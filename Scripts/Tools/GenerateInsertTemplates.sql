SELECT CASE
    -- Long line format
    WHEN (SELECT COUNT(name) FROM pragma_table_info(t.name)) > 3 THEN
        'INSERT INTO "' || t.name || '"' || char(10) ||
        '    (' || char(10) ||
                (
                    SELECT GROUP_CONCAT('        "' || name || '"', ',' || char(10))
                    FROM pragma_table_info(t.name)
                ) || char(10) ||
        '    )' || char(10) ||
        'VALUES' || char(10) ||
        '    (' || char(10) ||
                (
                    SELECT GROUP_CONCAT('        ?', ',' || char(10))
                    FROM pragma_table_info(t.name)
                ) || char(10) ||
        '    );' || char(10)
    -- Short line format
    ELSE
        'INSERT INTO "' || t.name || '"' || char(10) ||
        '        (' || (SELECT GROUP_CONCAT('"' || name || '"', ', ') FROM pragma_table_info(t.name)) || ')' || char(10) ||
        'VALUES  (?' || (SELECT GROUP_CONCAT(', ?', '') FROM pragma_table_info(t.name) WHERE cid > 0) || ');' || char(10)
END
FROM sqlite_master t
WHERE type = 'table' AND name NOT LIKE 'sqlite_%';
