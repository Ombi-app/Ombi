import { describe, it, expect } from 'vitest';
import { CustomizationSelectors } from './customization.selectors';

const mockSettings = {
  applicationName: 'Ombi',
  applicationUrl: 'https://ombi.local',
  logo: '/logo.png',
  customCssLink: '',
  enableCustomDonations: false,
  customDonationUrl: '',
  customDonationMessage: '',
  hasPresetTheme: false,
  presetThemeName: '',
  presetThemeContent: '',
  presetThemeDisplayName: '',
  presetThemeVersion: '',
  recentlyAddedPage: false,
};

describe('CustomizationSelectors', () => {
  it('should return full settings', () => {
    expect(CustomizationSelectors.customizationSettings(mockSettings as any)).toBe(mockSettings);
  });

  it('should select logo', () => {
    expect(CustomizationSelectors.logo(mockSettings as any)).toBe('/logo.png');
  });

  it('should select applicationName', () => {
    expect(CustomizationSelectors.applicationName(mockSettings as any)).toBe('Ombi');
  });

  it('should select applicationUrl', () => {
    expect(CustomizationSelectors.applicationUrl(mockSettings as any)).toBe('https://ombi.local');
  });
});
