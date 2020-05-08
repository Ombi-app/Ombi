import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { INavBar } from '../interfaces/ICommon';
import { StorageService } from '../shared/storage/storage-service';

@Component({
  selector: 'app-my-nav',
  templateUrl: './my-nav.component.html',
  styleUrls: ['./my-nav.component.scss'],
})
export class MyNavComponent implements OnInit {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

  @Input() public showNav: boolean;
  @Input() public applicationName: string;
  @Input() public username: string;
  @Input() public isAdmin: string;
  @Output() public logoutClick = new EventEmitter();
  @Output() public themeChange = new EventEmitter<string>();
  public theme: string;

  constructor(private breakpointObserver: BreakpointObserver,
              private store: StorageService) {
  }

  public ngOnInit(): void {
    this.theme = this.store.get("theme");
    if(!this.theme) {
      this.store.save("theme","dark");
    }
  }

  public navItems: INavBar[] = [
    { name: "NavigationBar.Discover", icon: "find_replace", link: "/discover", requiresAdmin: false },
    { name: "NavigationBar.Requests", icon: "list", link: "/requests-list", requiresAdmin: false },   
    { name: "NavigationBar.UserManagement", icon: "account_circle", link: "/usermanagement", requiresAdmin: true },
    { name: "NavigationBar.Calendar", icon: "calendar_today", link: "/calendar", requiresAdmin: false },
    { name: "NavigationBar.Settings", icon: "settings", link: "/Settings/About", requiresAdmin: true },
    { name: "NavigationBar.UserPreferences", icon: "person", link: "/user-preferences", requiresAdmin: false },
  ]

  public logOut() {
    this.logoutClick.emit();
  }

  public switchTheme() {
    if (this.theme) {
      let newTheme = "";
      if (this.theme === "dark") {
        newTheme = "light";
      } else {
        newTheme = "dark";
      }
      this.store.save("theme", newTheme)
      this.theme = newTheme;
      this.themeChange.emit(newTheme);
    }
  }

  // @TIDUSJAR Don't know if we need this method anymore?
  public getTheme(){
    return 'active-list-item';
  }

}
