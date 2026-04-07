import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SocialIconsComponent } from './social-icons.component';

describe('SocialIconsComponent', () => {
  function createComponent(baseHref = '/') {
    return new SocialIconsComponent(baseHref);
  }

  it('should set empty baseUrl when href is "/"', () => {
    const comp = createComponent('/');
    expect(comp.baseUrl).toBe('');
  });

  it('should set baseUrl when href is longer than 1 character', () => {
    const comp = createComponent('/ombi');
    expect(comp.baseUrl).toBe('/ombi');
  });

  it('should emit openTrailer event on openDialog', () => {
    const comp = createComponent();
    const spy = vi.fn();
    comp.openTrailer.subscribe(spy);

    comp.openDialog();
    expect(spy).toHaveBeenCalled();
  });

  it('should emit onAdvancedOptions event on openAdvancedOptions', () => {
    const comp = createComponent();
    const spy = vi.fn();
    comp.onAdvancedOptions.subscribe(spy);

    comp.openAdvancedOptions();
    expect(spy).toHaveBeenCalled();
  });

  it('should emit onReProcessRequest when reProcessRequest is called with is4K=false', () => {
    const comp = createComponent();
    const spy = vi.fn();
    const spy4K = vi.fn();
    comp.onReProcessRequest.subscribe(spy);
    comp.onReProcess4KRequest.subscribe(spy4K);

    comp.reProcessRequest(false);
    expect(spy).toHaveBeenCalled();
    expect(spy4K).not.toHaveBeenCalled();
  });

  it('should emit onReProcess4KRequest when reProcessRequest is called with is4K=true', () => {
    const comp = createComponent();
    const spy = vi.fn();
    const spy4K = vi.fn();
    comp.onReProcessRequest.subscribe(spy);
    comp.onReProcess4KRequest.subscribe(spy4K);

    comp.reProcessRequest(true);
    expect(spy4K).toHaveBeenCalled();
    expect(spy).not.toHaveBeenCalled();
  });
});
