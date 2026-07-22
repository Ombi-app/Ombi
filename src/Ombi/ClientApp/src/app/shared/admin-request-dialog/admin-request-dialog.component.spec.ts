import { describe, expect, it, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { AdminRequestDialogComponent } from './admin-request-dialog.component';
import { RequestType } from '../../interfaces';

function createComponent(is4k = false) {
  const dialogRef = { close: vi.fn() };
  const data = { type: RequestType.movie, id: 1, is4k, qualityOnly: true };
  const identity = { getUsersDropdown: vi.fn().mockReturnValue(of([])) };
  const sonarr = {};
  const radarr = {
    getQualityProfilesFromSettings: vi.fn().mockReturnValue(of([{ id: 1, name: 'HD' }])),
    getQualityProfiles4kFromSettings: vi.fn().mockReturnValue(of([{ id: 2, name: 'UHD' }])),
    getSelectableQualityProfilesFromSettings: vi.fn().mockReturnValue(of([{ id: 1, name: 'HD' }])),
    getSelectableQualityProfiles4kFromSettings: vi.fn().mockReturnValue(of([{ id: 2, name: 'UHD' }])),
    getRootFoldersFromSettings: vi.fn(),
    getRootFolders4kFromSettings: vi.fn(),
  };
  const fb = {
    group: vi.fn().mockReturnValue({
      value: { radarrPathId: null },
      controls: { username: { valueChanges: of('') } },
    }),
  };
  const facade = { isEnabled: vi.fn(), is4KEnabled: vi.fn() };
  const component = new AdminRequestDialogComponent(
    dialogRef as any, data, identity as any, sonarr as any, radarr as any,
    fb as any, facade as any, facade as any,
  );
  return { component, identity, radarr };
}

describe('AdminRequestDialogComponent quality-only mode', () => {
  it('loads only safe Sonarr profiles for TV', async () => {
    const dialogRef = { close: vi.fn() };
    const identity = { getUsersDropdown: vi.fn().mockReturnValue(of([])) };
    const sonarr = { getSelectableQualityProfilesWithoutSettings: vi.fn().mockReturnValue(of([{ id: 3, name: 'HD TV' }])) };
    const fb = { group: vi.fn().mockReturnValue({ value: {}, controls: { username: { valueChanges: of('') } } }) };
    const facade = { isEnabled: vi.fn(), is4KEnabled: vi.fn() };
    const component = new AdminRequestDialogComponent(
      dialogRef as any, { type: RequestType.tvShow, id: 1, is4k: null, qualityOnly: true }, identity as any,
      sonarr as any, {} as any, fb as any, facade as any, facade as any,
    );

    await component.ngOnInit();

    expect(component.sonarrProfiles).toEqual([{ id: 3, name: 'HD TV' }]);
    expect(identity.getUsersDropdown).not.toHaveBeenCalled();
    expect(sonarr.getSelectableQualityProfilesWithoutSettings).toHaveBeenCalled();
  });

  it('loads only safe standard Radarr profiles', async () => {
    const { component, identity, radarr } = createComponent();

    await component.ngOnInit();

    expect(component.radarrProfiles).toEqual([{ id: 1, name: 'HD' }]);
    expect(identity.getUsersDropdown).not.toHaveBeenCalled();
    expect(radarr.getRootFoldersFromSettings).not.toHaveBeenCalled();
    expect(radarr.getSelectableQualityProfilesFromSettings).toHaveBeenCalled();
    expect(radarr.getQualityProfilesFromSettings).not.toHaveBeenCalled();
  });

  it('uses the 4K profile endpoint for a 4K request', async () => {
    const { component, radarr } = createComponent(true);

    await component.ngOnInit();

    expect(radarr.getSelectableQualityProfiles4kFromSettings).toHaveBeenCalled();
    expect(radarr.getSelectableQualityProfilesFromSettings).not.toHaveBeenCalled();
  });

  it('falls back safely when Radarr profiles cannot be loaded', async () => {
    const { component, radarr } = createComponent();
    radarr.getSelectableQualityProfilesFromSettings.mockReturnValue(throwError(() => new Error('offline')));

    await component.ngOnInit();

    expect(component.profilesError).toBe(true);
    expect(component.profilesLoading).toBe(false);
    expect(component.form.value.radarrPathId).toBeNull();
  });
});
