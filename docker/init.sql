CREATE DATABASE "blumberg";
\connect blumberg;
CREATE USER "blumberg" WITH SUPERUSER PASSWORD 'blumberg';
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";