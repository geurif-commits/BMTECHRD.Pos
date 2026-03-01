param(
    [string]$BaseUrl = "http://localhost:5139",
    [string]$Username = "admin",
    [string]$Password = "admin",
    [string]$DeviceId = "PC-E2E-VALIDATOR",
    [string]$ActivationKey = "BMT-DEMO-00000",
    [decimal]$OpeningCash = 1000,
    [switch]$SkipActivate
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Invoke-Api {
    param(
        [ValidateSet('GET','POST','PUT','PATCH','DELETE')]
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )

    try {
        if ($null -eq $Body) {
            return Invoke-RestMethod -Uri $Url -Method $Method -Headers $Headers
        }

        $jsonBody = $Body | ConvertTo-Json -Depth 10
        return Invoke-RestMethod -Uri $Url -Method $Method -Headers $Headers -ContentType "application/json" -Body $jsonBody
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $responseText = $null

        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            $responseText = $_.ErrorDetails.Message
        }
        elseif ($_.Exception.Response) {
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                if ($stream) {
                    $reader = New-Object System.IO.StreamReader($stream)
                    $responseText = $reader.ReadToEnd()
                    $reader.Close()
                }
            }
            catch { }
        }

        Write-Host "[ERROR] $Method $Url -> HTTP $statusCode" -ForegroundColor Red
        if ($responseText) {
            Write-Host "[ERROR BODY] $responseText" -ForegroundColor DarkRed
        }

        throw
    }
}

function Ensure-LicenseActive {
    param(
        [string]$BusinessId,
        [hashtable]$Headers
    )

    if ($SkipActivate) {
        Write-Host "[INFO] SkipActivate habilitado. Se omite /api/license/activate"
        return
    }

    Write-Step "Activar licencia"
    $activate = Invoke-Api -Method POST -Url "$BaseUrl/api/license/activate" -Body @{ activationKey = $ActivationKey }
    Write-Host "Licencia -> status=$($activate.status) plan=$($activate.plan) expiresAt=$($activate.expiresAt)"

    Write-Step "Validar license/status y license/alerts"
    $status = Invoke-Api -Method GET -Url "$BaseUrl/api/license/status?businessId=$BusinessId" -Headers $Headers
    $alerts = Invoke-Api -Method GET -Url "$BaseUrl/api/license/alerts?businessId=$BusinessId" -Headers $Headers

    Write-Host "status: $($status.status) | plan: $($status.plan) | expiresAt: $($status.expiresAt)"
    if ($alerts -is [System.Array]) {
        Write-Host "alerts count: $($alerts.Count)"
    }
    else {
        Write-Host "alerts: $alerts"
    }
}

Write-Step "1) Obtener business público"
$public = Invoke-Api -Method GET -Url "$BaseUrl/api/business/public"
if ($public -is [System.Array]) {
    if ($public.Count -eq 0) { throw "No hay negocios en /api/business/public" }
    $business = $public[0]
}
else {
    $business = $public
}
$businessId = [string]$business.businessId
if ([string]::IsNullOrWhiteSpace($businessId)) {
    throw "No se pudo extraer businessId desde /api/business/public"
}
Write-Host "Business: $($business.name) | Id: $businessId"

Write-Step "2) Login"
$login = Invoke-Api -Method POST -Url "$BaseUrl/api/auth/login" -Body @{
    businessId = $businessId
    username = $Username
    password = $Password
    deviceId = $DeviceId
}

$accessToken = [string]$login.accessToken
if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw "Login no devolvió accessToken"
}

$headers = @{
    Authorization = "Bearer $accessToken"
    "X-Business-Id" = $businessId
}

Write-Host "Login OK (token recibido)"

Ensure-LicenseActive -BusinessId $businessId -Headers $headers

Write-Step "3) Obtener usuario actual"
$me = Invoke-Api -Method GET -Url "$BaseUrl/api/auth/me" -Headers $headers
$actorUserId = [string]$me.userId
if ([string]::IsNullOrWhiteSpace($actorUserId)) {
    throw "No se pudo obtener userId en /api/auth/me"
}
Write-Host "Actor: $($me.username) | role=$($me.role) | userId=$actorUserId"

Write-Step "4) Obtener o crear categoría/producto de prueba"
$categories = Invoke-Api -Method GET -Url "$BaseUrl/api/categories?businessId=$businessId" -Headers $headers
$category = $null
if ($categories -is [System.Array] -and $categories.Count -gt 0) {
    $category = $categories[0]
}

if ($null -eq $category) {
    $categoryName = "E2E-CAT-$(Get-Date -Format 'yyyyMMddHHmmss')"
    $category = Invoke-Api -Method POST -Url "$BaseUrl/api/categories" -Headers $headers -Body @{
        businessId = $businessId
        name = $categoryName
        sortOrder = 1
        isActive = $true
    }
    Write-Host "Categoría creada: $($category.name)"
}

$products = Invoke-Api -Method GET -Url "$BaseUrl/api/products?businessId=$businessId" -Headers $headers
$product = $null
if ($products -is [System.Array] -and $products.Count -gt 0) {
    $product = $products | Select-Object -First 1
}

if ($null -eq $product) {
    $productName = "E2E-PROD-$(Get-Date -Format 'yyyyMMddHHmmss')"
    $product = Invoke-Api -Method POST -Url "$BaseUrl/api/products" -Headers $headers -Body @{
        businessId = $businessId
        categoryId = $category.id
        name = $productName
        price = 150
        stock = 999
        trackInventory = $true
        area = "KITCHEN"
        isActive = $true
    }
    Write-Host "Producto creado: $($product.name)"
}

Write-Step "5) Seleccionar mesa y abrirla"
$tables = Invoke-Api -Method GET -Url "$BaseUrl/api/tables?businessId=$businessId" -Headers $headers
if (-not ($tables -is [System.Array]) -or $tables.Count -eq 0) {
    throw "No hay mesas para el negocio"
}

$table = $tables | Where-Object { $_.status -eq 'AVAILABLE' } | Select-Object -First 1
if ($null -eq $table) {
    $table = $tables | Select-Object -First 1
}

$tableId = [string]$table.id
Write-Host "Mesa elegida: #$($table.number) ($tableId) estado=$($table.status)"

if ($table.status -ne 'OPEN') {
    $null = Invoke-Api -Method POST -Url "$BaseUrl/api/tables/$tableId/open" -Headers $headers -Body @{
        waiterId = $actorUserId
    }
    Write-Host "Mesa abierta"
}

Write-Step "6) Crear comanda (orders/batch)"
$order = Invoke-Api -Method POST -Url "$BaseUrl/api/orders/batch" -Headers $headers -Body @{
    businessId = $businessId
    tableId = $tableId
    actorUserId = $actorUserId
    items = @(
        @{
            productId = $product.id
            quantity = 1
        }
    )
}
Write-Host "Order creada: $($order.orderId) | totalItems=$($order.totalItems)"

Write-Step "7) Consultar colas de cocina/bar"
$kitchen = Invoke-Api -Method GET -Url "$BaseUrl/api/kitchen/queue?businessId=$businessId" -Headers $headers
$bar = Invoke-Api -Method GET -Url "$BaseUrl/api/bar/queue?businessId=$businessId" -Headers $headers

$kCount = if ($kitchen -is [System.Array]) { $kitchen.Count } else { 0 }
$bCount = if ($bar -is [System.Array]) { $bar.Count } else { 0 }
Write-Host "Kitchen queue: $kCount | Bar queue: $bCount"

if ($kCount -gt 0) {
    $kid = [string]$kitchen[0].orderItemId
    $null = Invoke-Api -Method PATCH -Url "$BaseUrl/api/kitchen/items/$kid/status" -Headers $headers -Body @{ status = "IN_PROGRESS" }
    $null = Invoke-Api -Method PATCH -Url "$BaseUrl/api/kitchen/items/$kid/status" -Headers $headers -Body @{ status = "DONE" }
    Write-Host "Kitchen item $kid -> DONE"
}
elseif ($bCount -gt 0) {
    $bid = [string]$bar[0].orderItemId
    $null = Invoke-Api -Method PATCH -Url "$BaseUrl/api/bar/items/$bid/status" -Headers $headers -Body @{ status = "IN_PROGRESS" }
    $null = Invoke-Api -Method PATCH -Url "$BaseUrl/api/bar/items/$bid/status" -Headers $headers -Body @{ status = "DONE" }
    Write-Host "Bar item $bid -> DONE"
}

Write-Step "8) Apertura de turno y cobro en caja"
$activeShift = Invoke-Api -Method GET -Url "$BaseUrl/api/shifts/active?businessId=$businessId&userId=$actorUserId" -Headers $headers
$shiftId = [string]$activeShift.shiftId
if ([string]::IsNullOrWhiteSpace($shiftId) -or $activeShift.status -ne 'OPEN') {
    $openedShift = Invoke-Api -Method POST -Url "$BaseUrl/api/shifts/open" -Headers $headers -Body @{
        businessId = $businessId
        userId = $actorUserId
        openingCash = $OpeningCash
        notes = "E2E validation shift"
    }
    $shiftId = [string]$openedShift.shiftId
}
Write-Host "Shift activo: $shiftId"

$bill = Invoke-Api -Method GET -Url "$BaseUrl/api/cash/bill?businessId=$businessId&tableId=$tableId" -Headers $headers
$due = [decimal]$bill.due
if ($due -lt 0) { $due = 0 }
Write-Host "Bill total=$($bill.total) due=$due"

if ($due -gt 0) {
    $payment = Invoke-Api -Method POST -Url "$BaseUrl/api/cash/payments" -Headers $headers -Body @{
        businessId = $businessId
        tableId = $tableId
        shiftId = $shiftId
        actorUserId = $actorUserId
        method = "CASH"
        amount = $due
        cashGiven = $due
        closeIfPaid = $true
    }
    Write-Host "Pago OK -> paid=$($payment.paid) due=$($payment.due) closed=$($payment.closed)"
}

$close = Invoke-Api -Method POST -Url "$BaseUrl/api/cash/close" -Headers $headers -Body @{
    businessId = $businessId
    tableId = $tableId
}
Write-Host "Cierre de mesa: $($close.closed)"

Write-Step "VALIDACIÓN E2E COMPLETADA"
Write-Host "OK: login → licencia → mesas → comanda → cocina/bar → caja"
