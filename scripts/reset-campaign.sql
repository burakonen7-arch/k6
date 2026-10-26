-- Only campaign product 1 and its orders are reset. Stop load tests before executing.
BEGIN;
DELETE FROM "Orders" WHERE "ProductId" = 1;
UPDATE "Products" SET "Stock" = 100 WHERE "Id" = 1;
COMMIT;
