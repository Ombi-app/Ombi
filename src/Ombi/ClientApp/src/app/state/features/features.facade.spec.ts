import { describe, it, expect, beforeEach, vi } from 'vitest';
import { FeaturesFacade } from './features.facade';
import { LoadFeatures, EnableFeature, DisableFeature } from './features.actions';
import { of } from 'rxjs';

describe('FeaturesFacade', () => {
  let facade: FeaturesFacade;
  let mockStore: any;

  beforeEach(() => {
    mockStore = {
      select: vi.fn().mockReturnValue(of([])),
      dispatch: vi.fn().mockReturnValue(of(undefined)),
      selectSnapshot: vi.fn(),
    };

    // Use new with the mock store injected via constructor
    facade = new FeaturesFacade(mockStore);
  });

  it('should dispatch LoadFeatures on loadFeatures', () => {
    facade.loadFeatures();
    expect(mockStore.dispatch).toHaveBeenCalledWith(expect.any(LoadFeatures));
  });

  it('should dispatch EnableFeature on enable', () => {
    const feature = { name: 'Movie4KRequests', enabled: true };
    facade.enable(feature);
    expect(mockStore.dispatch).toHaveBeenCalledWith(expect.any(EnableFeature));
  });

  it('should dispatch DisableFeature on disable', () => {
    const feature = { name: 'Movie4KRequests', enabled: false };
    facade.disable(feature);
    expect(mockStore.dispatch).toHaveBeenCalledWith(expect.any(DisableFeature));
  });

  it('should call selectSnapshot for is4kEnabled', () => {
    mockStore.selectSnapshot.mockReturnValue(true);
    const result = facade.is4kEnabled();
    expect(mockStore.selectSnapshot).toHaveBeenCalled();
    expect(result).toBe(true);
  });

  it('should call selectSnapshot for isPlayedSyncEnabled', () => {
    mockStore.selectSnapshot.mockReturnValue(false);
    const result = facade.isPlayedSyncEnabled();
    expect(mockStore.selectSnapshot).toHaveBeenCalled();
    expect(result).toBe(false);
  });

  it('should call store.select for features$', () => {
    facade.features$();
    expect(mockStore.select).toHaveBeenCalled();
  });
});
