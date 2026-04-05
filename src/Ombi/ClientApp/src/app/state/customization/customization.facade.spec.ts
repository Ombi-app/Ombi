import { describe, it, expect, beforeEach, vi } from 'vitest';
import { CustomizationFacade } from './customization.facade';
import { LoadSettings, UpdateSettings } from './customization.actions';
import { of } from 'rxjs';

describe('CustomizationFacade', () => {
  let facade: CustomizationFacade;
  let mockStore: any;

  beforeEach(() => {
    mockStore = {
      select: vi.fn().mockReturnValue(of({})),
      dispatch: vi.fn().mockReturnValue(of(undefined)),
      selectSnapshot: vi.fn(),
    };

    facade = new CustomizationFacade(mockStore);
  });

  it('should dispatch LoadSettings on loadCustomizationSettings', () => {
    facade.loadCustomziationSettings();
    expect(mockStore.dispatch).toHaveBeenCalledWith(expect.any(LoadSettings));
  });

  it('should return logo from selectSnapshot', () => {
    mockStore.selectSnapshot.mockReturnValue('/custom-logo.png');
    expect(facade.logo()).toBe('/custom-logo.png');
  });

  it('should return appName from selectSnapshot', () => {
    mockStore.selectSnapshot.mockReturnValue('My Ombi');
    expect(facade.appName()).toBe('My Ombi');
  });

  it('should return appUrl from selectSnapshot', () => {
    mockStore.selectSnapshot.mockReturnValue('https://ombi.example.com');
    expect(facade.appUrl()).toBe('https://ombi.example.com');
  });

  it('should dispatch UpdateSettings on saveSettings', () => {
    facade.saveSettings({ applicationName: 'Test' } as any);
    expect(mockStore.dispatch).toHaveBeenCalledWith(expect.any(UpdateSettings));
  });

  it('should call store.select for settings$', () => {
    facade.settings$();
    expect(mockStore.select).toHaveBeenCalled();
  });
});
