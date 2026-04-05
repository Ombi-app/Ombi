import { describe, it, expect } from 'vitest';
import { LoadFeatures, EnableFeature, DisableFeature } from './features.actions';

describe('Feature Actions', () => {
  it('LoadFeatures should have correct type', () => {
    expect(LoadFeatures.type).toBe('[Features] LoadAll');
  });

  it('EnableFeature should store the feature and have correct type', () => {
    const feature = { name: 'Movie4KRequests', enabled: true };
    const action = new EnableFeature(feature);
    expect(EnableFeature.type).toBe('[Features] Enable');
    expect(action.feature).toEqual(feature);
  });

  it('DisableFeature should store the feature and have correct type', () => {
    const feature = { name: 'Movie4KRequests', enabled: false };
    const action = new DisableFeature(feature);
    expect(DisableFeature.type).toBe('[Features] Disable');
    expect(action.feature).toEqual(feature);
  });
});
