using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Data;

public static class Sqlite3Data
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-sqlite3",
        Title = "sqlite3：查看和使用 SQLite 数据库",
        Keywords = "sqlite3 · .tables · .schema · -header -column · .mode csv/json · 只读",
        Level = ModuleLevel.Intermediate,
        Overview = "很多程序（浏览器、KDE 组件、游戏启动器）用单个 SQLite 文件保存数据。sqlite3 可以打开这些文件查看表结构和内容，也可以自己建一个小数据库记录东西，再导出成 CSV 或 JSON。",

        Why =
        [
            "1. SQLite 数据库就是一个普通文件，没有服务器进程；复制文件就是复制整个数据库。",
            "2. 两种用法：",
            "- 交互式：`sqlite3 文件.db` 进入 `sqlite>` 提示符，输入 SQL 语句（以 `;` 结尾）或点命令，`.quit` 退出。",
            "- 单条执行：`sqlite3 文件.db \"SQL 语句\"`，结果输出后立即退出，适合脚本。",
            "3. 常用点命令：",
            "- `.tables`：列出所有表；`.schema 表名`：表结构。",
            "- `.mode column`（配合 `.headers on`）：对齐的表格；`.mode csv`、`.mode json`：导出格式。",
            "- 命令行参数 `-header -column`、`-csv`、`-json` 与之等价。",
            "4. 常用 SQL：`SELECT 列 FROM 表 WHERE 条件 ORDER BY 列 DESC LIMIT 10;`、`INSERT INTO 表 VALUES (...);`、`CREATE TABLE 表 (列 类型, ...);`。",
            "5. 查看其他程序的数据库时，**先复制一份再打开**，或者以只读方式打开：`sqlite3 'file:路径?mode=ro' ...`（需要加 `-cmd`，或在交互中使用 `.open --readonly 路径`）。程序正在运行时它可能锁定着数据库文件。",
        ],

        Consequences =
        [
            "• `SELECT` 与点命令只读；`INSERT`、`UPDATE`、`DELETE`、`DROP` 会立即写入文件，没有撤销。",
            "• 不要修改其他程序的数据库，可能导致它们无法启动或数据损坏。",
        ],

        Steps =
        [
            Step.Change(
                "创建一个数据库并写入数据",
                "mkdir -p /tmp/sql-demo && cd /tmp/sql-demo && sqlite3 games.db \"CREATE TABLE games(name TEXT, hours INT); INSERT INTO games VALUES ('Terraria',120),('Celeste',35),('Hades',60);\"",
                "文件不存在时自动创建。").Then("没有输出，当前目录出现 `games.db`。"),
            Step.Read(
                "以表格形式查询",
                "sqlite3 -header -column games.db 'SELECT * FROM games ORDER BY hours DESC;'",
                "`-header -column` 显示列名并对齐。").Then("三行结果按时长降序排列，第一行是 `Terraria  120`，上方有 `name  hours` 表头和分隔线。"),
            Step.Read(
                "查看表与表结构",
                "sqlite3 games.db '.tables' '.schema games'",
                "多个点命令依次执行。").Then("输出 `games` 与 `CREATE TABLE games(name TEXT, hours INT);`。"),
            Step.Read(
                "统计与筛选",
                "sqlite3 games.db 'SELECT count(*), sum(hours), avg(hours) FROM games; SELECT name FROM games WHERE hours > 50;'",
                "以分号分隔连续执行多条 SQL；第一条聚合统计总量与均值，第二条条件过滤满足记录。").Then("第一行 `3|215|71.66...`（数量、总和、平均），随后是 `Terraria` 与 `Hades`。"),
            Step.Change(
                "导出为 CSV 和 JSON",
                "sqlite3 -header -csv games.db 'SELECT * FROM games;' > games.csv && sqlite3 -json games.db 'SELECT * FROM games;' && cat games.csv",
                "`-csv` 与 `-json` 改变输出格式。").Then("先输出 JSON 数组 `[{\"name\":\"Terraria\",\"hours\":120}, ...]`，然后显示 CSV 文件内容 `name,hours`、`Terraria,120` 等。"),
            Step.Change(
                "先复制再查看其他程序的数据库（示例）",
                "cp <数据库文件> /tmp/copy.db && sqlite3 /tmp/copy.db '.tables'",
                "`<数据库文件>` 为目标 sqlite 文件路径（如浏览器 `History`）。先复制再看，避免读写锁冲突。").Then("列出该数据库中所有表的名称；再用 `.schema <表名>` 或 `SELECT * FROM <表名> LIMIT 5;` 查看具体内容。"),
        ],

        Verify =
        [
            "1. `sqlite3 games.db 'SELECT count(*) FROM games;'` 输出 3。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/sql-demo` 清理。",
        ],
    };
}
