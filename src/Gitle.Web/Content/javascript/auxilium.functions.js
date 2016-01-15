var growl = function (type, text) {
  var alert = $('<div>').addClass("alert-box");
  $("body").prepend(alert.addClass(type).html(text));
  alert.delay(2000).fadeOut(2000, function () {
    $(this).remove();
  });
};

var slugify = function (origin, target) {
  $(origin).change(function () {
    $.ajax({
      method: 'POST',
      data: {
        text: $(this).val()
      },
      url: '/slug/index',
      success: function (data) {
        $(target).val(data);
      }
    });

  }).change();
};
