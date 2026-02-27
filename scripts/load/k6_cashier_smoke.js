import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 5,
  duration: '20s',
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1200'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const BUSINESS_ID = __ENV.BUSINESS_ID || '';

export default function () {
  const url = `${BASE_URL}/api/cash/tables?businessId=${BUSINESS_ID}`;
  const res = http.get(url);

  check(res, {
    'status is 200/401/403': (r) => [200, 401, 403].includes(r.status),
  });

  sleep(1);
}
