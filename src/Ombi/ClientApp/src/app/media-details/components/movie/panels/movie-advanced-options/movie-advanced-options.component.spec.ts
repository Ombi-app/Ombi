import { describe, it, expect, vi } from 'vitest';
import { MovieAdvancedOptionsComponent } from './movie-advanced-options.component';
import { RequestCombination } from '../../../../../interfaces';
import { of } from 'rxjs';

function createComponent(requestCombination = RequestCombination.Normal) {
  const mockDialogRef = { close: vi.fn() };
  const data = {
    movieRequest: { requestCombination, qualityOverride: 1, rootPathOverride: 2, qualityOverrideTitle: '', rootPathOverrideTitle: '' },
    profiles: [],
    rootFolders: [],
  };
  const mockRadarrService = {
    getQualityProfilesFromSettings: vi.fn().mockReturnValue(of([{ id: 1, name: 'HD-1080p' }])),
    getRootFoldersFromSettings: vi.fn().mockReturnValue(of([{ id: 2, path: '/movies' }])),
    getQualityProfiles4kFromSettings: vi.fn().mockReturnValue(of([{ id: 3, name: 'UHD' }])),
    getRootFolders4kFromSettings: vi.fn().mockReturnValue(of([{ id: 4, path: '/movies4k' }])),
  };

  const comp = new MovieAdvancedOptionsComponent(mockDialogRef as any, data as any, mockRadarrService as any);
  return { comp, data, mockRadarrService };
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

  it('should show both for Both request combination', async () => {
    const { comp } = createComponent(RequestCombination.Both);
    await comp.ngOnInit();
    expect(comp.show4k).toBe(true);
    expect(comp.showNormal).toBe(true);
  });
});
