const https = require('https');
const data = JSON.stringify({
  email: 'saadaouiilef1@gmail.com',
  password: 'Admin@123'
});

const req = https.request({
  hostname: 'localhost',
  port: 7075,
  path: '/api/auth/login',
  method: 'POST',
  rejectUnauthorized: false, // Ignore self-signed cert errors
  headers: {
    'Content-Type': 'application/json',
    'Content-Length': data.length
  }
}, (res) => {
  console.log('STATUS:', res.statusCode);
  res.setEncoding('utf8');
  res.on('data', (chunk) => {
    console.log('BODY:', chunk);
  });
});

req.on('error', (e) => {
  console.error(`problem with request: ${e.message}`);
});

req.write(data);
req.end();
