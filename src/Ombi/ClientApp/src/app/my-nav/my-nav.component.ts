import { Component, Input, Output, EventEmitter, HostBinding } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { INavBar } from '../interfaces/ICommon';

@Component({
  selector: 'app-my-nav',
  templateUrl: './my-nav.component.html',
  styleUrls: ['./my-nav.component.scss'],
})
export class MyNavComponent {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

  @Input() public showNav: boolean;
  @Input() public applicationName: string;
  @Input() public username: string;
  @Output() public logoutClick = new EventEmitter();
  @Output() public themeChange = new EventEmitter<string>();


  constructor(private breakpointObserver: BreakpointObserver) {
  }

  public navItems: INavBar[] = [
    { name: "NavigationBar.Discover", icon: "find_replace", link: "/discover" },
    { name: "NavigationBar.Requests", icon: "list", link: "/requests" },
    { name: "NavigationBar.Settings", icon: "settings", link: "/Settings/About" },
  ]

  public logOut() {
    this.logoutClick.emit();
  }

  public switchTheme() {
    const theme = localStorage.getItem("theme");

    if (theme) {
      localStorage.removeItem("theme");
      let newTheme = "";
      if (theme === "dark") {
        newTheme = "light";
      } else {
        newTheme = "dark";
      }
      localStorage.setItem("theme", newTheme)
      this.themeChange.emit(newTheme);
    }
  }

}
