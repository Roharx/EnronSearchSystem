CREATE TABLE occurrences (
                             word_id INT NOT NULL,
                             file_id INT NOT NULL,
                             count INT NOT NULL,
                             created_at DATE NOT NULL DEFAULT CURRENT_DATE
) PARTITION BY RANGE (created_at);

-- Create partitions for each month
DO $$ 
DECLARE r RECORD;
BEGIN
FOR r IN SELECT generate_series(0, 12) AS i LOOP
             EXECUTE format(
            'CREATE TABLE occurrences_%s PARTITION OF occurrences 
             FOR VALUES FROM (DATE ''2025-01-01'') TO (DATE ''2025-02-01'')',
             r.i
        );
END LOOP;
END $$;
