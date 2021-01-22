import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { INavBar } from '../interfaces/ICommon';
import { StorageService } from '../shared/storage/storage-service';
import { SettingsService } from '../services';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { SearchFilter } from './SearchFilter';
import { Md5 } from 'ts-md5/dist/md5';
import { RequestType } from '../interfaces';
import { FilterService } from '../discover/services/filter-service';

export enum SearchFilterType {
  Movie = 1,
  TvShow = 2,
  Music = 3,
  People = 4
}

@Component({
  selector: 'app-my-nav',
  templateUrl: './my-nav.component.html',
  styleUrls: ['./my-nav.component.scss'],
})
export class MyNavComponent implements OnInit {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe([Breakpoints.Small, Breakpoints.Handset, Breakpoints.XSmall])
    .pipe(
      map(result => result.matches)
    );

  @Input() public showNav: boolean;
  @Input() public applicationName: string;
  @Input() public applicationLogo: string;
  @Input() public username: string;
  @Input() public isAdmin: string;
  @Input() public email: string;
  @Output() public logoutClick = new EventEmitter();
  public theme: string;
  public issuesEnabled: boolean = false;
  public navItems: INavBar[];
  public searchFilter: SearchFilter;
  public SearchFilterType = SearchFilterType;
  public emailHash: string | Int32Array;
  public welcomeText: string;
  public RequestType = RequestType;

  constructor(private breakpointObserver: BreakpointObserver,
    private settingsService: SettingsService,
    private store: StorageService,
    private filterService: FilterService) {
  }

  public async ngOnInit() {

    this.searchFilter = {
      movies: true,
      music: false,
      people: false,
      tvShows: true
    }

    this.setWelcomeText();
    if (this.email) {
      const md5 = new Md5();
      this.emailHash = md5.appendStr(this.email).end();
    }

    this.issuesEnabled = await this.settingsService.issueEnabled().toPromise();
    const customizationSettings = await this.settingsService.getCustomization().toPromise();
    console.log("issues enabled: " + this.issuesEnabled);
    this.theme = this.store.get("theme");
    if (!this.theme) {
      this.store.save("theme", "dark");
    }
    var filter = this.store.get("searchFilter");
    if (filter) {
      this.searchFilter = Object.assign(new SearchFilter(), JSON.parse(filter));
      this.filterService.changeFilter(this.searchFilter);
    }
    this.navItems = [
      { name: "NavigationBar.Discover", icon: "find_replace", link: "/discover", requiresAdmin: false, enabled: true, faIcon: null },
      { name: "NavigationBar.Requests", icon: "list", link: "/requests-list", requiresAdmin: false, enabled: true, faIcon: null },
      { name: "NavigationBar.Issues", icon: "notification_important", link: "/issues", requiresAdmin: false, enabled: this.issuesEnabled, faIcon: null },
      { name: "NavigationBar.UserManagement", icon: "account_circle", link: "/usermanagement", requiresAdmin: true, enabled: true, faIcon: null },
      // { name: "NavigationBar.Calendar", icon: "calendar_today", link: "/calendar", requiresAdmin: false, enabled: true },
      { name: "NavigationBar.Donate", icon: "attach_money", link: "https://www.paypal.me/PlexRequestsNet", externalLink: true, requiresAdmin: true, enabled: true, toolTip: true, style: "color:red;", toolTipMessage: 'NavigationBar.DonateTooltip', faIcon: null },
      { name: "NavigationBar.Donate", icon: "attach_money", link: customizationSettings.customDonationUrl, externalLink: true, requiresAdmin: false, enabled: customizationSettings.enableCustomDonations, toolTip: true, toolTipMessage: customizationSettings.customDonationMessage, faIcon: null },
      { name: "NavigationBar.FeatureSuggestion", icon: null, link: "https://features.ombi.io/", externalLink: true, requiresAdmin: true, enabled: true, toolTip: true, toolTipMessage: 'NavigationBar.FeatureSuggestionTooltip', faIcon: "fa-lightbulb-o" },
      { name: "NavigationBar.Settings", icon: "settings", link: "/Settings/About", requiresAdmin: true, enabled: true, faIcon: null },
      { name: "NavigationBar.UserPreferences", icon: "person", link: "/user-preferences", requiresAdmin: false, enabled: true, faIcon: null },
    ];
  }

  public logOut() {
    this.logoutClick.emit();
  }

  public changeFilter(event: MatSlideToggleChange, searchFilterType: SearchFilterType) {
    switch (searchFilterType) {
      case SearchFilterType.Movie:
        this.searchFilter.movies = event.checked;
        break;
      case SearchFilterType.TvShow:
        this.searchFilter.tvShows = event.checked;
        break;
      case SearchFilterType.Music:
        this.searchFilter.music = event.checked;
        break;
      case SearchFilterType.People:
        this.searchFilter.people = event.checked;
        break;
    }
    this.filterService.changeFilter(this.searchFilter);
    this.store.save("searchFilter", JSON.stringify(this.searchFilter));
  }

  private setWelcomeText() {
    var d = new Date();
    var hour = d.getHours();

    if (hour >= 0 && hour < 12) {
      this.welcomeText = 'NavigationBar.MorningWelcome';
    }
    if (hour >= 12 && hour < 18) {
      this.welcomeText = 'NavigationBar.AfternoonWelcome';
    }
    if (hour >= 18 && hour < 24) {
      this.welcomeText = 'NavigationBar.EveningWelcome';
    }
  }
}
