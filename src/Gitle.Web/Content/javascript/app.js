


var growl = function(type, text) {
  var alert = $('<div>').addClass("alert-box");
  $("body").prepend(alert.addClass(type).html(text));
  alert.delay(2000).fadeOut(2000, function() {
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

function Application() {
}

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

    $('#initiate-joyride').click(function () {
      $(document).foundation('joyride', 'start');
    });

    $('.tablesorter').tablesorter({});

    self.initMilestoneSelect();
    self.initFreckleSelect();
    
    slugify('#item_Name', '#item_Slug')
  },

  initOnLoad: function () {

  },

  initFreckleSelect: function () {
    $('#item_FreckleId').change(function () {
      $('#item_FreckleName').val($(this).find('option:selected').html());
    }).change();
  },

  initMilestoneSelect: function () {
    $("#item_Repository").change(function () {
      var fullrepo = $(this).val();
      var url = '/milestone/select?fullrepo=' + fullrepo + '&projectSlug=' + $('#item_Slug').val();
      $('.milestone-select').load(url);
    }).change();
    $('form').on('change', '#item_MilestoneId', function () {
      $('#item_MilestoneName').val($(this).find('option:selected').html());
    }).change();
  }
};

var app = null;
$(document).ready(function() {
  app = new Application();
  app.init();
});

$(window).load(function() {
  app.initOnLoad();
});