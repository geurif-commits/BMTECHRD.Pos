param(
    [string]$BaseUrl = 'http://localhost:41750'
)

$evidenceDir = Join-Path -Path (Get-Location) -ChildPath ("evidence\verify_auth_e2e_$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')")
New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
Write-Output "Base URL: $BaseUrl"
Write-Output "Evidence dir: $evidenceDir"

# Get public businesses
try {
    $biz = Invoke-RestMethod -Uri "$BaseUrl/api/business/public" -Method Get -ErrorAction Stop
    $biz | ConvertTo-Json -Depth 10 | Out-File "$evidenceDir\businesses.json"
    if ($biz.Length -eq 0) { throw "No businesses returned" }
    $businessId = $biz[0].businessId
    Write-Output "Found businessId: $businessId"
} catch {
    Write-Error "Failed to get public businesses: $_"
    exit 2
}

# Login
$loginPayload = @{ businessId = $businessId; username = 'admin'; password = 'admin'; deviceId = 'integration-device-e2e' } | ConvertTo-Json
try {
    $loginResp = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginPayload -ContentType 'application/json' -UseBasicParsing -ErrorAction Stop
    $loginResp.Content | Out-File "$evidenceDir\login_response.json"
    $loginResp.StatusCode.ToString() | Out-File "$evidenceDir\login_status.txt"
    $loginObj = $loginResp.Content | ConvertFrom-Json
    $accessToken = $loginObj.accessToken
    $refreshToken = $loginObj.refreshToken
    if (-not $accessToken) { throw 'No accessToken in login response' }
    Write-Output "Login OK"
} catch {
    Write-Error "Login failed: $_"
    exit 3
}

# Me
try {
    $meResp = Invoke-WebRequest -Uri "$BaseUrl/api/auth/me" -Method Get -Headers @{ Authorization = "Bearer $accessToken" } -UseBasicParsing -ErrorAction Stop
    $meResp.Content | Out-File "$evidenceDir\me_response.json"
    $meResp.StatusCode.ToString() | Out-File "$evidenceDir\me_status.txt"
    Write-Output "Me OK"
} catch {
    Write-Error "Me failed: $_"
    exit 4
}

# Refresh
$refreshPayload = @{ refreshToken = $refreshToken; deviceId = 'integration-device-e2e' } | ConvertTo-Json
try {
    $refreshResp = Invoke-WebRequest -Uri "$BaseUrl/api/auth/refresh" -Method Post -Body $refreshPayload -ContentType 'application/json' -UseBasicParsing -ErrorAction Stop
    $refreshResp.Content | Out-File "$evidenceDir\refresh_response.json"
    $refreshResp.StatusCode.ToString() | Out-File "$evidenceDir\refresh_status.txt"
    Write-Output "Refresh OK"
} catch {
    Write-Error "Refresh failed: $_"
    exit 5
}

Write-Output "Saved evidence to $evidenceDir"
