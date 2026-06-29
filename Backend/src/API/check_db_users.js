const { Client } = require('pg')

const client = new Client({
  host: 'localhost',
  port: 5433,
  database: 'novahiredb2',
  user: 'postgres',
  password: 'root'
})

async function main() {
  await client.connect()
  console.log('Connected to DB\n')

  // Check users
  const users = await client.query(
    `SELECT email, role, "IsActive", "EmailConfirmed", 
     LEFT("PasswordHash", 7) as hash_type,
     "CompanyId"
     FROM "Users" 
     ORDER BY role, email 
     LIMIT 30`
  )
  console.log('=== USERS ===')
  console.table(users.rows)

  // Check companies
  const companies = await client.query(
    `SELECT "Name", "IsActive", "Status" FROM "Companies" LIMIT 10`
  )
  console.log('\n=== COMPANIES ===')
  console.table(companies.rows)

  await client.end()
}

main().catch(e => {
  console.error('ERROR:', e.message)
  process.exit(1)
})
