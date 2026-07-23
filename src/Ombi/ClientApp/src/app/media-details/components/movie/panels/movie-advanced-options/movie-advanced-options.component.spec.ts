import { describe, it, expect, vi } from 'vitest';
import { MovieAdvancedOptionsComponent } from './movie-advanced-options.component';
import { RequestCombination } from '../../../../../interfaces';
import { of } from 'rxjs';

function createComponent(requestCombination = RequestCombination.Normal, canSelectQualityProfile = true) {
  const mockDialogRef = { close: vi.fn() };
  const data = {
    movieRequest: { requestCombination, qualityOverride: 1, qualityOverride4K: 3, rootPathOverride: 2, qualityOverrideTitle: '', rootPathOverrideTitle: '' },
    profiles: [],
    profiles4K: [],
    profileId: undefined as number | undefined,
    profileId4K: undefined as number | undefined,
    rootFolders: [],
  };
  const mockRadarrService = {
    getQualityProfilesFromSettings: vi.fn().mockReturnValue(of([{ id: 1, name: 'HD-1080p' }])),
    getRootFoldersFromSettings: vi.fn().mockReturnValue(of([{ id: 2, path: '/movies' }])),
    getQualityProfiles4kFromSettings: vi.fn().mockReturnValue(of([{ id: 3, name: 'UHD' }])),
    getRootFolders4kFromSettings: vi.fn().mockReturnValue(of([{ id: 4, path: '/movies4k' }])),
  };
  const mockAuthService = {
    hasRole: vi.fn().mockImplementation((role: string) => canSelectQualityProfile && role === 'SelectRadarrQualityProfile'),
  };

  const comp = new MovieAdvancedOptionsComponent(mockDialogRef as any, data as any, mockRadarrService as any, mockAuthService as any);
  return { comp, data, mockRadarrService, mockAuthService };
}

describe('MovieAdvancedOptionsComponent', () => {
  it('should load normal profiles for Normal request', async () => {
    const { comp, mockRadarrService } = createComponent(RequestCombination.Normal);
    await comp.ngOnInit();
    expect(comp.showNormal).toBe(true);
    expect(comp.show4k).toBe(false);
    expect(mockRadarrService.getQualityProfilesFromSettings).toHaveBeenCalled();
    expect(mockRadarrService.getRootFoldersFromSettings).toHaveBeenCalled();
  });

  it('should load 4K profiles for FourK request', async () => {
    const { comp, mockRadarrService } = createComponent(RequestCombination.FourK);
    await comp.ngOnInit();
    expect(comp.show4k).toBe(true);
    expect(mockRadarrService.getQualityProfiles4kFromSettings).toHaveBeenCalled();
    expect(mockRadarrService.getRootFolders4kFromSettings).toHaveBeenCalled();
  });

  it('should set quality override title when matching profile exists', async () => {
    const { comp, data } = createComponent(RequestCombination.Normal);
    await comp.ngOnInit();
    expect(data.movieRequest.qualityOverrideTitle).toBe('HD-1080p');
  });

  it('should set root path override title when matching folder exists', async () => {
    const { comp, data } = createComponent(RequestCombination.Normal);
    await comp.ngOnInit();
    expect(data.movieRequest.rootPathOverrideTitle).toBe('/movies');
  });

  it('should load independent normal and 4K profiles for Both request combination', async () => {
    const { comp, data, mockRadarrService } = createComponent(RequestCombination.Both);
    await comp.ngOnInit();
    expect(comp.show4k).toBe(true);
    expect(comp.showNormal).toBe(true);
    expect(mockRadarrService.getQualityProfilesFromSettings).toHaveBeenCalled();
    expect(mockRadarrService.getQualityProfiles4kFromSettings).toHaveBeenCalled();
    expect(data.profiles).toEqual([{ id: 1, name: 'HD-1080p' }]);
    expect(data.profiles4K).toEqual([{ id: 3, name: 'UHD' }]);
    expect(data.profileId).toBe(1);
    expect(data.profileId4K).toBe(3);
  });

  it('should not expose or load quality profile controls without the selector role', async () => {
    const { comp, mockRadarrService } = createComponent(RequestCombination.Both, false);

    await comp.ngOnInit();

    expect(comp.canSelectRadarrQualityProfile).toBe(false);
    expect(mockRadarrService.getQualityProfilesFromSettings).not.toHaveBeenCalled();
    expect(mockRadarrService.getQualityProfiles4kFromSettings).not.toHaveBeenCalled();
    expect(mockRadarrService.getRootFolders4kFromSettings).toHaveBeenCalled();
  });
});
