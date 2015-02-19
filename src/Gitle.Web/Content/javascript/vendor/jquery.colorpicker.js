jQuery.fn.colorPicker = function (conf) {
  // Config for plug
  var config = jQuery.extend({
    class: 'colorpicker-popup',    // class of color-picker popup
    title: 'Kies een kleur'        // Default popup title
  }, conf);

  var colors = ["ffffff", "ffccc9", "ffce93", "fffc9e", "ffffc7", "9aff99", "96fffb", "cdffff", "cbcefb", "cfcfcf", "fd6864", "fe996b", "fffe65", "fcff2f", "67fd9a", "38fff8", "68fdff", "9698ed", "c0c0c0", "fe0000", "f8a102", "ffcc67", "f8ff00", "34ff34", "68cbd0", "34cdf9", "6665cd", "9b9b9b", "cb0000", "f56b00", "ffcb2f", "ffc702", "32cb00", "00d2cb", "3166ff", "6434fc", "656565", "9a0000", "ce6301", "cd9934", "999903", "009901", "329a9d", "3531ff", "6200c9", "343434", "680100", "963400", "986536", "646809", "036400", "34696d", "00009b", "303498", "000000", "330001", "643403", "663234", "343300", "013300", "003532", "010066", "340096"];

  // Add the color-picker dialogue if not added
  var colorPicker = jQuery('.' + config.class);

  if (!colorPicker.length) {
    colorPicker = jQuery('<div class="' + config.class + '"></div>').appendTo(document.body).hide();

    // Remove the color-picker if you click outside it (on body)
    jQuery(document.body).click(function (event) {
      if (!(jQuery(event.target).is('.' + config.class) || jQuery(event.target).parents('.' + config.class).length)) {
        colorPicker.hide();
      }
    });
  }

  // For every input passed to the plug-in
  return this.each(function () {
    // Get icon and input
    var input = jQuery(this);
    var icon = jQuery('.colorpicker-open[rel="'+input.prop('name')+'"]');
    var loc = '';

    for (var i = 0; i < colors.length; i++) {
      var hex = colors[i];
      var title = colors[i];
      loc += '<li><a href="#" title="'
                    + title
                    + '" rel="'
                    + hex
                    + '" style="background: #'
                    + hex
                    + ';">'
                    + title
                    + '</a></li>';
    }

    input.change(function () {
      icon.css('color', '#' + input.val());
    });

    input.change();

    // When you click the icon
    icon.click(function () {
      // Show the color-picker next to the icon and fill it with the colors
      var iconPos = icon.offset();
      var heading = config.title ? '<h2>' + config.title + '</h2>' : '';

      colorPicker.html(heading + '<ul>' + loc + '</ul>').css({
        position: 'absolute',
        left: iconPos.left + 'px',
        top: iconPos.top + 'px'
      }).show();

      // When you click a color in the color-picker
      jQuery('a', colorPicker).click(function () {
        // The hex is stored in the link's rel-attribute
        var hex = jQuery(this).prop('rel');

        input.val(hex);

        icon.css('color', '#' + hex);

        input.change();

        colorPicker.hide();

        return false;
      });

      return false;
    });
  });
};