
const { Client } = require('pg');

const client = new Client({
  host: 'localhost',
  port: 5433,
  database: 'novahiredb',
  user: 'postgres',
  password: 'root',
});

async function checkUsers() {
  try {
    await client.connect();
    console.log('Connected to database');
    const res = await client.query('SELECT "Email", "Role", "IsActive", "PasswordHash" FROM "Users"');
    console.log('Users found:');
    console.table(res.rows);
  } catch (err) {
    console.error('Error connecting to database:', err);
  } finally {
    await client.end();
  }
}

checkUsers();
