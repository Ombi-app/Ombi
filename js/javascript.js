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

// Document Ready
$(document).ready(function() {
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
            scrollTop: target.offset().top - 70 // Adjusted for fixed header
          }, 800, function() {
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

  // Fit text for responsive headings
  $("h1").fitText(1.2, {
    'maxFontSize': 48
  });

  // Remove lightbox initialization
  // var lightbox = $('.screenshots a').simpleLightbox({
  //   nav: true,
  //   navText: ['<i class="fas fa-chevron-left"></i>', '<i class="fas fa-chevron-right"></i>'],
  //   closeText: '<i class="fas fa-times"></i>',
  //   showCounter: false
  // });

  // Improved scroll animations for elements with animation classes
  function revealOnScroll() {
    var scrolled = $(window).scrollTop();
    var windowHeight = $(window).height();

    $('.fade-in, .slide-up').each(function() {
      var $this = $(this);
      var offsetTop = $this.offset().top;

      // Trigger animation when element is 200px from entering viewport
      if (scrolled + windowHeight > offsetTop - 200) {
        $this.addClass('animated');
      }
    });
  }

  // Run once immediately to show elements above the fold
  revealOnScroll();

  // Run again after a short delay to catch all elements
  setTimeout(revealOnScroll, 100);

  // Run on scroll with throttling for performance
  var scrollThrottleTimeout;
  $(window).on('scroll', function() {
    if (!scrollThrottleTimeout) {
      scrollThrottleTimeout = setTimeout(function() {
        revealOnScroll();
        scrollThrottleTimeout = null;
      }, 50);
    }
  });

  // Navbar scroll effect
  $(window).scroll(function() {
    if ($(window).scrollTop() > 50) {
      $('#navbar').addClass('nav-scrolled');
    } else {
      $('#navbar').removeClass('nav-scrolled');
    }
  });
});

// only after all images have loaded
$(window).on('load', function() {
  $('body').addClass('load-done'); // Show content once loaded
  
  // Enhanced Background slideshow with smooth transitions
  (function($) {
    'use strict';
    var $slides = $('[data-slides]');
    var images = $slides.data('slides');
    var count = images.length;
    var current = 0;
    var nextImage = 1;
    
    // Create a second background element for crossfade effect
    var $nextSlide = $('<div class="intro-bg intro-bg-next"></div>');
    $nextSlide.insertAfter($slides);
    
    // Initial setup
    $slides.css('background-image', 'url("' + images[0] + '")');
    $nextSlide.css({
      'background-image': 'url("' + images[1] + '")',
      'opacity': 0
    });
    
    // Enhanced slideshow with crossfade
    var slideshow = function() {
      nextImage = (current + 1) % count;
      
      // Set the next slide's background
      $nextSlide.css('background-image', 'url("' + images[nextImage] + '")');
      
      // Fade in next slide
      $nextSlide.animate({opacity: 1}, 1000, function() {
        // Update current slide with next image (behind the scenes)
        $slides.css('background-image', 'url("' + images[nextImage] + '")');
        
        // Reset next slide opacity
        $nextSlide.css('opacity', 0);
        
        // Update current index
        current = nextImage;
        
        // Queue next slide transition
        setTimeout(slideshow, 5000);
      });
    };
    
    // Start the slideshow after a delay
    setTimeout(slideshow, 5000);
  }(jQuery));
});
