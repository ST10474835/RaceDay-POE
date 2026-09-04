-- =========================================================
-- RaceDay Verification Queries
-- These queries confirm the database, tables, and sample
-- data were created correctly.
-- =========================================================

-- 1. Confirm all tables exist with row counts
SELECT
    t.name AS TableName,
    p.rows AS RowCount
FROM sys.tables t
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
ORDER BY t.name;
