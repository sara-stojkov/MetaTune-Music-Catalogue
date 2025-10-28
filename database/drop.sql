-- PostgreSQL Drop Script for Pluralized Schema
-- Tables are dropped in reverse dependency order to avoid foreign key errors.

-- 1. Drop Tables that depend on many others (ratings, reviews, tasks, etc.)
DROP TABLE IF EXISTS ratings;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS tasks;

-- 2. Drop Junction/Associative Tables
DROP TABLE IF EXISTS qualifications;
DROP TABLE IF EXISTS contributors;
DROP TABLE IF EXISTS performs;
DROP TABLE IF EXISTS belongs;
DROP TABLE IF EXISTS members;

-- 3. Drop Main Entity Tables with Internal Dependencies
-- Works references genres and self-references (albumId)
DROP TABLE IF EXISTS works;

-- Authors references people
DROP TABLE IF EXISTS authors;

-- Genres self-references (parentGenreId)
DROP TABLE IF EXISTS genres;

-- Users references people
DROP TABLE IF EXISTS users;

-- 4. Drop Core Entity Tables
DROP TABLE IF EXISTS people;


-- ----------------------------------------
-- Drop Unique Indexes
-- (These will typically be dropped with the table, but explicitly listing 
-- unique indexes is good practice if they weren't created as PRIMARY KEYs)
-- ----------------------------------------

DROP INDEX IF EXISTS authors_personId_idx;
DROP INDEX IF EXISTS users_personId_idx;
