import { Component, Input, Output, EventEmitter } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';


@Component({
  selector: 'app-my-nav',
  templateUrl: './my-nav.component.html',
  styleUrls: ['./my-nav.component.css']
})
export class MyNavComponent {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches)
    );

    @Input() public showNav: boolean;
    @Input() public loggedIn: boolean;
    @Input() public username: string;
    @Output() public logoutClick = new EventEmitter();

  constructor(private breakpointObserver: BreakpointObserver) {
  }

  public logOut() {
    this.logoutClick.emit();
  }

  public isLoggedIn(): boolean {
    return this.loggedIn;
  }
}
