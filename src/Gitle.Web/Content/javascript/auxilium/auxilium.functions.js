/*
 * Convert value from current elemente into slug and put it in target element [data-slugify]
 */
$.fn.slugify = function () {
  return this.each(function() {
    $(this).change(function () {
      var target = $($(this).data('slugify'));
      $.ajax({
        method: 'POST',
        data: {
          text: $(this).val()
        },
        url: '/slug/index',
        success: function (data) {
          target.val(data);
        }
      });

    }).change();
  });
};

/*
 * Insert text string into textarea at cursor
 */
$.fn.insert = function (text) {
  return this.each(function () {
    var textarea = $(this);
    if (textarea[0].nodeName === "TEXTAREA") {
      textarea.val(textarea.val().substr(0, textarea.prop("selectionStart")) + text + textarea.val().substr(textarea.prop("selectionEnd")));
    }
  });
};

/*
 * Replace all dots into commas
 */
$.fn.commafy = function() {
  return this.replace('.', ',');
};

/*
 * Replace all commas into dots
 */
$.fn.dotify = function() {
  return this.replace(',', '.');
};

$.fn.currency = function() {
  return parseFloat(this).toFixed(2).commafy();
};

/*
 * Show given error below element
 */
$.fn.error = function(error) {
  return this.each(function () {
    $(this).siblings('small.error').remove();
    $(this).after('<small class="error">' + error + '</small>').parent().addClass("error");
  });
}

/*
 * Make given field remember its value by cookies
 */
$.fn.cookieRemember = function() {
  return this.each(function() {
    var name = $(this).data('remember');
    if ($.cookie(name) && self.queryString['query'] == undefined) {
      $(this).val($.cookie(name)).parents('form').submit();
    }
    $.cookie(name, $(this).val());
  }).change(function() {
    var name = $(this).data('remember');
    $.cookie(name, $(this).val());
  });
};

/*
 * Set Ctrl + S to save the forms on the page
 */
$.fn.ctrlS = function() {
  return this.keydown(function(e) {
    if (!(e.which == 83 && e.ctrlKey)) return true;
    $("form").submit();
    e.preventDefault();
    return false;
  });
};

/*
 * Make rows in a table clickable (gets the first link found in the row)
 */
$.fn.tableRowClickable = function() {
  return this.each(function() {
    $(this).find('tr').click(function() {
      var href = $(this).find("a").first().attr("href");
      if (href) {
        window.location = href;
      }
    });
  });
};

/*
 * Markdownify the contents of the element
 */
$.fn.markdownify = function() {
  marked.setOptions({
    breaks: true
  });
  return this.each(function() {
    $(this).html(marked($(this).html()));
  });
};

var queryString = [];
var match,
        pl = /\+/g,  // Regex for replacing addition symbol with a space
        search = /([^&=]+)=?([^&]*)/g,
        decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
        query = window.location.search.substring(1);

while (match = search.exec(query))
  queryString[decode(match[1])] = decode(match[2]);