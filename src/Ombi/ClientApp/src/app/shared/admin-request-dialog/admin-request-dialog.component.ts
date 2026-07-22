import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { RadarrFacade } from 'app/state/radarr';
import { SonarrFacade } from 'app/state/sonarr';
import { firstValueFrom, Observable } from 'rxjs';
import { startWith, map } from 'rxjs/operators';
import {
	ILanguageProfiles,
	IRadarrProfile,
	IRadarrRootFolder,
	ISonarrProfile,
	ISonarrRootFolder,
	IUserDropdown,
	RequestType,
} from '../../interfaces';
import { IdentityService, RadarrService, SonarrService } from '../../services';

export interface IAdminRequestDialogData {
	type: RequestType;
	id: number;
	is4k: boolean | null;
	qualityOnly?: boolean;
}

@Component({
    standalone: true,
	selector: 'admin-request-dialog',
	templateUrl: 'admin-request-dialog.component.html',
	styleUrls: ['admin-request-dialog.component.scss'],
	imports: [
		CommonModule,
		ReactiveFormsModule,
		MatAutocompleteModule,
		MatButtonModule,
		MatDialogModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
		MatProgressSpinnerModule,
		TranslateModule
	]
})
export class AdminRequestDialogComponent implements OnInit {
	constructor(
		public dialogRef: MatDialogRef<AdminRequestDialogComponent>,
		@Inject(MAT_DIALOG_DATA) public data: IAdminRequestDialogData,
		private identityService: IdentityService,
		private sonarrService: SonarrService,
		private radarrService: RadarrService,
		private fb: UntypedFormBuilder,
		private sonarrFacade: SonarrFacade,
		private radarrFacade: RadarrFacade,
	) {}

	public form: UntypedFormGroup;
	public RequestType = RequestType;

	public options: IUserDropdown[];
	public filteredOptions: Observable<IUserDropdown[]>;
	public userId: string;

	public radarrEnabled: boolean;
	public radarr4kEnabled: boolean;
	public sonarrEnabled: boolean;

	public sonarrProfiles: ISonarrProfile[];
	public sonarrRootFolders: ISonarrRootFolder[];
	public sonarrLanguageProfiles: ILanguageProfiles[];
	public radarrProfiles: IRadarrProfile[];
	public radarrRootFolders: IRadarrRootFolder[];
	public profilesLoading = false;
	public profilesError = false;

	public async ngOnInit() {
		this.form = this.fb.group({
			username: [null],
			sonarrPathId: [null],
			sonarrFolderId: [null],
			sonarrLanguageId: [null],
			radarrPathId: [null],
			radarrFolderId: [null],
		});

		if (!this.data.qualityOnly) {
			this.options = await firstValueFrom(this.identityService.getUsersDropdown());

			this.filteredOptions = this.form.controls['username'].valueChanges.pipe(
				startWith(''),
				map((value) => this._filter(value)),
			);
		}

		if (this.data.type === RequestType.tvShow) {
			if (this.data.qualityOnly) {
				this.sonarrEnabled = true;
				this.loadSonarrProfiles();
				return;
			}
			this.sonarrEnabled = this.sonarrFacade.isEnabled();
			if (this.sonarrEnabled) {
				console.log(this.sonarrFacade.version());
				if (this.sonarrFacade.version()[0] === '3') {
					this.sonarrService.getV3LanguageProfilesWithoutSettings().subscribe((profiles: ILanguageProfiles[]) => {
						this.sonarrLanguageProfiles = profiles;
					});
				}
				this.sonarrService.getQualityProfilesWithoutSettings().subscribe((c) => {
					this.sonarrProfiles = c;
				});
				this.sonarrService.getRootFoldersWithoutSettings().subscribe((c) => {
					this.sonarrRootFolders = c;
				});
			}
		}
		if (this.data.type === RequestType.movie) {
			if (this.data.qualityOnly) {
				this.radarrEnabled = true;
				this.loadRadarrProfiles();
				return;
			}
			this.radarrEnabled = this.radarrFacade.isEnabled();
			this.radarr4kEnabled = this.radarrFacade.is4KEnabled();

			if (this.data.is4k ?? false) {
				if (this.radarr4kEnabled) {
					this.radarrService.getQualityProfiles4kFromSettings().subscribe((c) => {
						this.radarrProfiles = c;
					});
					this.radarrService.getRootFolders4kFromSettings().subscribe((c) => {
						this.radarrRootFolders = c;
					});
				}
			} else {
				if (this.radarrEnabled) {
					this.radarrService.getQualityProfilesFromSettings().subscribe((c) => {
						this.radarrProfiles = c;
					});
					this.radarrService.getRootFoldersFromSettings().subscribe((c) => {
						this.radarrRootFolders = c;
					});
				}
			}
		}
	}

	private loadRadarrProfiles(): void {
		this.profilesLoading = true;
		const profiles = this.data.is4k
			? this.radarrService.getSelectableQualityProfiles4kFromSettings()
			: this.radarrService.getSelectableQualityProfilesFromSettings();
		profiles.subscribe({
			next: (result) => {
				this.radarrProfiles = result;
				this.profilesLoading = false;
			},
			error: () => {
				this.profilesError = true;
				this.profilesLoading = false;
			},
		});
	}

	private loadSonarrProfiles(): void {
		this.profilesLoading = true;
		this.sonarrService.getSelectableQualityProfilesWithoutSettings().subscribe({
			next: (result) => {
				this.sonarrProfiles = result;
				this.profilesLoading = false;
			},
			error: () => {
				this.profilesError = true;
				this.profilesLoading = false;
			},
		});
	}

	public displayFn(user: IUserDropdown): string {
		const username = user?.username ? user.username : '';
		const email = user?.email ? `(${user.email})` : '';
		if (username || email) {
			return `${username} ${email}`;
		}
		return '';
	}

	private _filter(value: string | IUserDropdown): IUserDropdown[] {
		const filterValue = typeof value === 'string' ? value.toLowerCase() : value.username.toLowerCase();

		return this.options.filter((option) => option.username.toLowerCase().includes(filterValue));
	}

	public async submitRequest() {
		const model = this.form.value;
		model.radarrQualityOverrideTitle = this.radarrProfiles?.filter((x) => x.id == model.radarrPathId)[0]?.name;
		model.radarrRootFolderTitle = this.radarrRootFolders?.filter((x) => x.id == model.radarrFolderId)[0]?.path;
		model.sonarrRootFolderTitle = this.sonarrRootFolders?.filter((x) => x.id == model.sonarrFolderId)[0]?.path;
		model.sonarrQualityOverrideTitle = this.sonarrProfiles?.filter((x) => x.id == model.sonarrPathId)[0]?.name;
		model.sonarrLanguageProfileTitle = this.sonarrLanguageProfiles?.filter((x) => x.id == model.sonarrLanguageId)[0]?.name;
		this.dialogRef.close(model);
	}
}
