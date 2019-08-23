docker run -d -p 5432:5432 --name roadkill-postgres -e POSTGRES_USER=roadkill -e POSTGRES_PASSWORD=roadkill postgres


// CI version
- docker run -d -p 5432:5432 --name roadkill-postgres postgres
- docker exec roadkill-postgres psql -c 'create database roadkill;' -U postgres

Alternatively install Postgres, and set the root password to "roadkill"
