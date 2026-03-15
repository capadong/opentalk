-- Migration: add file_type column to files table
-- Run this if upgrading an existing database (schema already has the files table without file_type)
-- Note: MySQL 8.0 does not support ADD COLUMN IF NOT EXISTS; check first before running.
ALTER TABLE files ADD COLUMN file_type VARCHAR(128) NOT NULL DEFAULT '' AFTER file_path;
