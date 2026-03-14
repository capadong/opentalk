-- Migration: add file_type column to files table
-- Run this if upgrading an existing database (schema already has the files table without file_type)
ALTER TABLE files ADD COLUMN IF NOT EXISTS file_type VARCHAR(128) NOT NULL DEFAULT '' AFTER file_path;
