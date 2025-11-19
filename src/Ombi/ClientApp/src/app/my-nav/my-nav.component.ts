import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges, signal, computed } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatRippleModule } from '@angular/material/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule, MatSlideToggleChange } from '@angular/material/slide-toggle';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ICustomizationSettings, IUser, RequestType, UserType } from '../interfaces';
import { LidarrService, SettingsService, SettingsStateService } from '../services';

import { AdvancedSearchDialogComponent } from '../shared/advanced-search-dialog/advanced-search-dialog.component';
import { CustomizationFacade } from '../state/customization';
import { FilterService } from '../discover/services/filter-service';
import { ILocalUser } from '../auth/IUserLogin';
import { INavBar } from '../interfaces/ICommon';
import { Md5 } from 'ts-md5/dist/md5';
import { Observable } from 'rxjs';
import { SearchFilter, DEFAULT_SEARCH_FILTER } from './SearchFilter';
import { StorageService } from '../shared/storage/storage-service';
import { map, take } from 'rxjs/operators';
import { RemainingRequestsComponent } from '../shared/remaining-requests/remaining-requests.component';
import { NavSearchComponent } from './nav-search.component';

export enum SearchFilterType {
  Movie = 1,
  TvShow = 2,
  Music = 3,
  People = 4
}

@Component({
    standalone: true,
    selector: 'app-my-nav',
    templateUrl: './my-nav.component.html',
    styleUrls: ['./my-nav.component.scss'],
    imports: [
        CommonModule,
        MatButtonModule,
        MatDialogModule,
        MatIconModule,
        MatListModule,
        MatMenuModule,
        MatRippleModule,
        MatSidenavModule,
        MatSlideToggleModule,
        MatToolbarModule,
        MatTooltipModule,
        RouterModule,
        TranslateModule,
        RemainingRequestsComponent,
        NavSearchComponent
    ]
})
export class MyNavComponent implements OnInit {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe([Breakpoints.Small, Breakpoints.Handset, Breakpoints.XSmall])
    .pipe(
      map(result => result.matches)
    );

  @Input() public showNav: boolean;
  @Input() public applicationName: string;
  @Input() public applicationLogo: string;
  @Input() public applicationUrl: string;
  @Input() public accessToken: string;
  @Input() public userName: string;
  @Input() public userEmail: string;
  @Input() public isAdmin: string;
  @Output() public logoutClick = new EventEmitter();
  public theme: string;
  public issuesEnabled: boolean = false;
  public navItems: INavBar[];
  public searchFilter = signal<SearchFilter>({ ...DEFAULT_SEARCH_FILTER });
  public SearchFilterType = SearchFilterType;
  public userProfileImageUrl: string;
  public welcomeText: string;
  public RequestType = RequestType;

  private customizationSettings: ICustomizationSettings;

  public readonly musicEnabled$ = this.lidarrService.enabled().pipe(take(1));

  constructor(private breakpointObserver: BreakpointObserver,
    private settingsService: SettingsService,
    private customizationFacade: CustomizationFacade,
    private lidarrService: LidarrService,
    private store: StorageService,
    private filterService: FilterService,
    private dialogService: MatDialog,
    private readonly settingState: SettingsStateService,
    private router: Router) {
  }

  public async ngOnInit() {
    this.searchFilter.set({ ...DEFAULT_SEARCH_FILTER });

    this.setProfileImageUrl(this.userEmail)
    this.issuesEnabled = await this.settingsService.issueEnabled().toPromise();
    this.settingState.setIssue(this.issuesEnabled);

    this.customizationFacade.settings$().subscribe(settings => {
      this.customizationSettings = settings;
    });

    this.theme = this.store.get("theme");
    if (!this.theme) {
      this.store.save("theme", "dark");
    }
    var filter = this.store.get("searchFilter");
    if (filter) {
      this.searchFilter.set(Object.assign(new SearchFilter(), JSON.parse(filter)));
      this.filterService.changeFilter(this.searchFilter());
    }
    this.navItems = [
      { id: "nav-discover", name: "NavigationBar.Discover", icon: "fas fa-bolt", style:"z-index:-1;", link: "/discover", requiresAdmin: false, enabled: true },
      { id: "nav-requests", name: "NavigationBar.Requests", icon: "fas fa-stream", link: "/requests-list", requiresAdmin: false, enabled: true },
      { id: "nav-issues", name: "NavigationBar.Issues", icon: "fas fa-exclamation-triangle", link: "/issues", requiresAdmin: false, enabled: this.issuesEnabled },
      { id: "nav-userManagement", name: "NavigationBar.UserManagement", icon: "fas fa-users", link: "/usermanagement", requiresAdmin: true, enabled: true },
      //id: "",  { name: "NavigationBar.Calendar", icon: "calendar_today", link: "/calendar", requiresAdmin: false, enabled: true },
      { id: "nav-adminDonate", name: "NavigationBar.Donate", icon: "fas fa-dollar-sign", link: "https://www.paypal.me/PlexRequestsNet", externalLink: true, requiresAdmin: true, enabled: true, toolTip: true, style: "color:red;", toolTipMessage: 'NavigationBar.DonateTooltip' },
      { id: "nav-userDonate", name: "NavigationBar.Donate", icon: "fas fa-dollar-sign", link: this.customizationSettings.customDonationUrl, externalLink: true, requiresAdmin: false, enabled: this.customizationSettings.enableCustomDonations, toolTip: true, toolTipMessage: this.customizationSettings.customDonationMessage },
      { id: "nav-featureSuggestion", name: "NavigationBar.FeatureSuggestion", icon: "far fa-lightbulb", link: "https://features.ombi.io/", externalLink: true, requiresAdmin: true, enabled: true, toolTip: true, toolTipMessage: 'NavigationBar.FeatureSuggestionTooltip'},
      { id: "nav-settings", name: "NavigationBar.Settings", icon: "fas fa-cogs", link: "/Settings/About", requiresAdmin: true, enabled: true },
      ];
  }

  ngOnChanges(changes: SimpleChanges) {
    if(changes?.userEmail || changes?.applicationLogo){
      this.setProfileImageUrl(this.userEmail)
    }
  }

  public logOut() {
    this.logoutClick.emit();
  }

  public changeFilter(event: MatSlideToggleChange, searchFilterType: SearchFilterType) {
    const updated = { ...this.searchFilter() };

    switch (searchFilterType) {
      case SearchFilterType.Movie:
        updated.movies = event.checked;
        break;
      case SearchFilterType.TvShow:
        updated.tvShows = event.checked;
        break;
      case SearchFilterType.Music:
        updated.music = event.checked;
        break;
      case SearchFilterType.People:
        updated.people = event.checked;
        break;
    }

    this.searchFilter.set(updated);
    this.filterService.changeFilter(this.searchFilter());
    this.store.save("searchFilter", JSON.stringify(this.searchFilter()));
  }

  public activeFilterCount = computed(() => {
    return Object.values(this.searchFilter()).filter(value => value).length;
  });

  public hasNonDefaultFilters = computed(() => {
    const current = this.searchFilter();
    for (const key in current) {
      if (current[key] !== DEFAULT_SEARCH_FILTER[key]) {
        return true;
      }
    }
    return false;
  });

  public openAdvancedSearch() {
    const dialogRef = this.dialogService.open(AdvancedSearchDialogComponent, { panelClass: 'dialog-responsive' });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.router.navigate([`discover/advanced/search`]);
      }

      return;
    });
  }

  private setProfileImageUrl(email: string): void {
    if (email) {
        const md5 = new Md5();
        const emailHash = md5.appendStr(email).end();
        this.userProfileImageUrl = `https://www.gravatar.com/avatar/${emailHash}?d=404`;;
    }
    else {
        this.userProfileImageUrl = this.getFallbackProfileImageUrl();
    }
  }

  public onProfileImageError(): void {
      const fallbackLogo = this.getFallbackProfileImageUrl();
      if (this.userProfileImageUrl === fallbackLogo) return;        
      this.userProfileImageUrl = fallbackLogo;
  }

  private getFallbackProfileImageUrl() {
      return this.applicationLogo
        ? this.applicationLogo
        : "https://raw.githubusercontent.com/Ombi-app/Ombi/gh-pages/img/android-chrome-512x512.png";
  }

  public openMobileApp(event: any) {
    event.preventDefault();

    const url = `ombi://${this.applicationUrl}|${this.accessToken}`;
    window.location.assign(url);
  }
}
