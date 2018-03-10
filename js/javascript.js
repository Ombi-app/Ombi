(function($) {
  $.fn.fitText = function(kompressor, options) {
    var compressor = kompressor || 1,
      settings = $.extend({
        'minFontSize': Number.NEGATIVE_INFINITY,
        'maxFontSize': Number.POSITIVE_INFINITY
      }, options);
    return this.each(function() {
      var $this = $(this);
      var resizer = function() {
        $this.css('font-size', Math.max(Math.min($this.width() / (compressor * 10), parseFloat(settings.maxFontSize)), parseFloat(settings.minFontSize)));
      };
      resizer();
      $(window).on('resize.fittext orientationchange.fittext', resizer);
    });
  };
})($);

// Smooth scrolling when clicking anchor
$('a[href*="#"]')
  .not('[href="#"]')
  .not('[href="#0"]')
  .click(function(event) {
    if (
      location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') &&
      location.hostname == this.hostname
    ) {
      var target = $(this.hash);
      target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
      if (target.length) {
        event.preventDefault();
        $('html, body').animate({
          scrollTop: target.offset().top
        }, 1000, function() {
          var $target = $(target);
          $target.focus();
          if ($target.is(":focus")) {
            return false;
          } else {
            $target.attr('tabindex', '-1');
            $target.focus();
          };
        });
      }
    }
  });

// Fit long h1 on small screen
$("h1").fitText(1.2, {
  'maxFontSize': 30
});

// enable screenshot lightbox
var lightbox = $('.screenshots a').simpleLightbox();

// only after all images have loaded
$(window).on('load', function() {
  $('body').addClass('load-done'); // unfold triangles etc
  /*! slides | https://gist.github.com/mhulse/66bcbb7099bb4beae530 */
  (function($) {
    'use strict';
    var $slides = $('[data-slides]');
    var images = $slides.data('slides');
    var count = images.length;
    var slideshow = function() {
      $slides
        .css('background-image', 'url("' + images[Math.floor(Math.random() * count)] + '")')
        .show(0, function() {
          setTimeout(slideshow, 5000);
        });
    };
    slideshow();
  }(jQuery));
})
