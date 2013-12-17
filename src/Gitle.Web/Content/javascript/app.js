


var growl = function (type, text) {
    var alert = $('<div>').addClass("alert-box");
    $("body").prepend(alert.addClass(type).html(text));
    alert.delay(2000).fadeOut(2000, function () {
        $(this).remove();
    });
}


function Application() { }

Application.prototype = {
    init: function () {
        var self = this;

        $(document.body).addClass("js-enabled");


        $("table.row-clickable tr").click(function () {
            var href = $(this).find("a").first().attr("href");
            if (href) {
                window.location = href;
            }
        });

    },

    initOnLoad: function () {

    }


};

var app = null;
$(document).ready(function () {
    app = new Application();
    app.init();
});

$(window).load(function () {
    app.initOnLoad();
});
