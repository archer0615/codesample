取得Table中不同欄位的最大值
```sql
--1,2,3是table一筆資料中的三個不同欄位
SELECT MAX(num) AS maxOfNums
FROM (VALUES (1), (2), (3)) AS tblNums(num);
```