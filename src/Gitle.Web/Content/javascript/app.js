$.fn.extend({
  insert: function (text) {
    return this.each(function () {
      var textarea = $(this);
      if (textarea[0].nodeName == "TEXTAREA") {
        textarea.val(textarea.val().substr(0, textarea.prop("selectionStart")) + text + textarea.val().substr(textarea.prop("selectionEnd")));
      }
    });
  },
  error: function (error) {
    return this.each(function () {
      $(this).after('<small class="error">' + error + '</small>').parent().addClass("error");
    });
  },
  upload: function () {
    return this.each(function () {
      var textarea = $(this);
      var progressbar = $('<div class="progress">').append($('<span class="meter">')).insertAfter(this).hide();
      if (!$.browser.msie)
        textarea.after($('<small class="info">Je kunt ook afbeeldingen uploaden door ze op het textveld te slepen.</small>'));
      var uploadButton = $('<a href="#" class="button">Upload afbeelding</a>').insertBefore(this);
      var uploadOptions = {
        url: '/upload/file',
        beforeSend: function (e, a) {
          progressbar.show();
        },
        onprogress: function (e) {
          if (e.lengthComputable) {
            console.log('Progress ' + e.loaded + ' of ' + e.total);
            var percent = e.loaded / e.total * 100;
            progressbar.find('.meter').css('width', percent + '%');
          }
        },
        error: function () {
          console.log('error');
          textarea.error('Bestand(en) uploaden niet gelukt');
        },
        success: function (data) {
          data = $.parseJSON(data);
          for (var upload in data.Uploads) {
            textarea.insert('![alt](' + data.Uploads[upload] + ')');
          }
          for (var error in data.Errors) {
            console.log('Error: ' + error + ' - ' + data.Errors[error]);
            textarea.error(error + ': ' + data.Errors[error]);
          }
          progressbar.hide();
        }
      }
      $(this).ajaxUploadDrop(uploadOptions);
      uploadButton.ajaxUploadPrompt(uploadOptions);
    });
  }
});

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

    slugify('#item_Name', '#item_Slug');

    $('.uploadarea').upload();

    marked.setOptions({
      breaks: true
    })
    $('.marked').each(function () {
      $(this).html(marked($(this).html()));
    })

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