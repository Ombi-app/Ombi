import { describe, it, expect } from 'vitest';
import { FeaturesSelectors } from './features.selectors';

const mockFeatures = [
  { name: 'Movie4KRequests', enabled: true },
  { name: 'PlayedSync', enabled: false },
];

describe('FeaturesSelectors', () => {
  it('should return all features', () => {
    expect(FeaturesSelectors.features(mockFeatures as any)).toBe(mockFeatures);
  });

  it('should return true when Movie4KRequests is enabled', () => {
    expect(FeaturesSelectors.is4kEnabled(mockFeatures as any)).toBe(true);
  });

  it('should return false when PlayedSync is disabled', () => {
    expect(FeaturesSelectors.isPlayedSyncEnabled(mockFeatures as any)).toBe(false);
  });
});
