<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
    <head>
    <title>Check status</title>
    <script type="text/javascript" src="../../../../Scripts/jquery-1.4.4.js"></script>
    <script type="text/javascript" src="../../../../Scripts/raphael-min.js"></script>
    <script type="text/javascript" language="javascript">
        var paper;
        $(
            function () {
                paper = Raphael("tasks", 640, 400);
                setInterval("update_status()", 5000);
                update_status();
            }
        );

            function update_status() {
                $.ajax({
                    url: "data",
                    success: function data(msg) {
                        switch (msg.status.Overall) {
                            case 0: set_status("Build started", "yellow");
                                break;
                            case 1: set_status("Build ended with a failure", "red");
                                break;
                            case 2: set_status("Build ended with success", "green");
                                break;
                            default:
                                set_status("Unknown build status", "cyan");
                        }

                        var tasks = msg.status.Tasks;
                        update_tasks(tasks);
                        update_terminal_output();
                    },
                    error: function () {
                        $("#status").text("Nothing to see here, moving along");
                    }
                });
            };
        function set_status(msg, color) {
            $("#status").text(msg);
            $("body").attr("style", "background : " + color + ";");
        };

        function get_console_output(taskIndex) {
            $.ajax({
                url: "task/" + taskIndex,
                success: function (msg) {
                    $("#terminal").text(msg.terminalOutput);
                },
                error : function() {
                    console.log("Error retriving the terminal output");
                }
            });
        };

        function update_terminal_output() {
            $.ajax({
                url: "task",
                success: function (msg) {
                    $("#terminal").text(msg.terminalOutput);
                },
                error: function () {
                    console.log("Error retriving the terminal output");
                }
            });
        }

        function update_tasks(tasks) {
            paper.clear();
            var taskStatusToColor = { 0: "gray", 1: "yellow", 2: "green", 3: "red" };
            for (var i = 0; i < tasks.length; i++) {
                var r = paper.rect(-50, -25, 100, 50);
                var text = paper.text(0, 0, tasks[i].Name);
                text.attr("fill", "#f1f1f1");
                text.attr("font-size", "18");
                text.attr("font-family", "Helvetica");
                r.attr("fill", taskStatusToColor[tasks[i].Status]);
                var st = paper.set();
                st.push(r);
                st.push(text);
                st.translate(100 * (i + 1), 100);
            }
        };
    </script>
    </head>
    <body>
        <div id="status"></div>
        <div id="tasks"></div>
        <textarea id="terminal" style="width:100%;height:30%;margin-bottom:0;" rows="20" readonly="readonly"></textarea>
    </body>
</html>