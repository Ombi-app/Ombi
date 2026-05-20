import { describe, it, expect } from 'vitest';
import { LoadSettings, UpdateSettings } from './customization.actions';

describe('Customization Actions', () => {
  it('LoadSettings should have correct type', () => {
    expect(LoadSettings.type).toBe('[Customization] LoadSettings');
  });

  it('UpdateSettings should store settings and have correct type', () => {
    const settings = {
      applicationName: 'Ombi',
      applicationUrl: 'http://localhost',
      logo: '/logo.png',
      customCss: '',
      enableCustomDonations: false,
      customDonationUrl: '',
      customDonationMessage: '',
      recentlyAddedPage: true,
      useCustomPage: false,
      hideAvailableFromDiscover: false,
      favicon: '',
      id: 1,
    };
    const action = new UpdateSettings(settings as any);
    expect(UpdateSettings.type).toBe('[Customization] UpdateSettings');
    expect(action.settings).toEqual(settings);
  });
});
