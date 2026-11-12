-- Runs once, when the Postgres volume is first created. Keycloak keeps its tables in this schema.
CREATE SCHEMA IF NOT EXISTS keycloak;
