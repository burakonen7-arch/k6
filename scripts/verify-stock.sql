DO $$
BEGIN
 IF (SELECT "Stock" FROM "Products" WHERE "Id" = 1) <> 0
 OR (SELECT count(*) FROM "Orders" WHERE "ProductId" = 1) <> 100
 OR (SELECT coalesce(sum("Quantity"),0) FROM "Orders" WHERE "ProductId" = 1) <> 100 THEN
 RAISE EXCEPTION 'Stock invariant violated';
 END IF;
END $$;
