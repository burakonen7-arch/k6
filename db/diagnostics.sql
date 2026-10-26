SELECT query, calls, round(mean_exec_time::numeric,2) AS mean_ms,
 round(total_exec_time::numeric,2) AS total_ms, rows
FROM pg_stat_statements ORDER BY total_exec_time DESC LIMIT 15;
SELECT state, wait_event_type, wait_event, count(*) FROM pg_stat_activity
WHERE datname = current_database() GROUP BY state, wait_event_type, wait_event;
EXPLAIN (ANALYZE, BUFFERS) SELECT "Id", "Name", "Price", "Stock"
FROM "Products" WHERE "Id" > 50000 ORDER BY "Id" LIMIT 20;
