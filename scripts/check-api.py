"""Running API integration checks. python3 scripts/check-api.py [base_url]"""
import json
import sys
import urllib.error
import urllib.request
base = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:8080'
def request(path, expected, data=None):
    req = urllib.request.Request(base + path, data=json.dumps(data).encode() if data is not None else None, headers={'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=20) as response:
            code, body = response.status, response.read()
    except urllib.error.HTTPError as error:
        code, body = error.code, error.read()
    assert code == expected, (path, code, body)
    return json.loads(body) if body else None
request('/health/ready', 200)
request('/api/products?limit=101', 400)
request('/api/products/100001', 404)
request('/api/orders', 400, {'productId': 2, 'quantity': 0})
request('/api/orders', 409, {'productId': 100001, 'quantity': 1})
order = request('/api/orders', 201, {'productId': 2, 'quantity': 1})
assert request('/api/orders/' + order['id'], 200)['quantity'] == 1
assert '/api/orders' in request('/swagger/v1/swagger.json', 200)['paths']
print('PASS: readiness, validation, missing resource, conflict, create/get and Swagger')
