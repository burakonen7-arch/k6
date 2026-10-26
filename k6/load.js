import http from 'k6/http';
import { check, fail } from 'k6';
const base = __ENV.BASE_URL || 'http://localhost:8080';
const mode = __ENV.MODE || 'smoke';
const rate = Number(__ENV.RATE || 100);
const scenarios = {
 smoke: { executor: 'constant-vus', vus: 2, duration: '10s' },
 load: { executor: 'constant-arrival-rate', rate, timeUnit: '1s', duration: __ENV.DURATION || '60s', preAllocatedVUs: 50, maxVUs: 300 },
 spike: { executor: 'ramping-arrival-rate', startRate: 10, timeUnit: '1s', preAllocatedVUs: 50, maxVUs: 500, stages: [{ target: 50, duration: '20s' }, { target: 500, duration: '10s' }, { target: 500, duration: '30s' }, { target: 10, duration: '10s' }] },
 stock: { executor: 'shared-iterations', vus: 50, iterations: 200, maxDuration: '30s' }
};
if (!scenarios[mode]) throw new Error('MODE must be smoke, load, spike or stock');
http.setResponseCallback(http.expectedStatuses(200, 201, 409));
export const options = {
 scenarios: { traffic: scenarios[mode] },
 thresholds: { checks: ['rate==1'], http_req_failed: ['rate<0.01'], http_req_duration: ['p(95)<500', 'p(99)<1500'], dropped_iterations: ['count==0'] },
 summaryTrendStats: ['avg', 'p(90)', 'p(95)', 'p(99)', 'max']
};
export function setup() {
 const ready = http.get(`${base}/health/ready`);
 if (ready.status !== 200) fail('API is not ready');
 if (mode === 'stock') {
  const p = http.get(`${base}/api/products/1`).json();
  if (p.stock !== 100) fail('stock scenario requires product 1 stock = 100; reset lab data first');
 }
}
export default function () {
 if (mode === 'stock') {
  const r = http.post(`${base}/api/orders`, JSON.stringify({ productId: 1, quantity: 1 }), { headers: { 'Content-Type': 'application/json' }, tags: { name: 'flash-order' } });
  check(r, { 'created or sold out': r => r.status === 201 || r.status === 409 });
  return;
 }
 const choice = Math.random();
 if (choice < 0.6) {
  const r = http.get(`${base}/api/products?afterId=${Math.floor(Math.random()*99000)}&limit=20`, { tags: { name: 'product-list' } });
  check(r, { 'list OK': r => r.status === 200 && r.json().length === 20 });
 } else if (choice < 0.9) {
  const r = http.get(`${base}/api/products/${2+Math.floor(Math.random()*1000)}`, { tags: { name: 'product-detail' } });
  check(r, { 'detail OK': r => r.status === 200 });
 } else {
  const r = http.post(`${base}/api/orders`, JSON.stringify({ productId: 2+Math.floor(Math.random()*1000), quantity: 1 }), { headers: { 'Content-Type': 'application/json' }, tags: { name: 'create-order' } });
  check(r, { 'order created': r => r.status === 201 });
 }
}
export function teardown() {
 if (mode === 'stock') {
  const r = http.get(`${base}/api/products/1`);
  check(r, { 'stock exactly zero': r => r.status === 200 && r.json().stock === 0 });
 }
}
