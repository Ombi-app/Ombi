import { BreakpointObserver, Breakpoints, BreakpointState } from "@angular/cdk/layout";
import { Component } from "@angular/core";
import { Observable } from "rxjs";

@Component({
    selector: "app-nav",
    templateUrl: "./nav.component.html",
    styleUrls: ["./nav.component.scss"],
})
export class NavComponent {
    public isHandset: Observable<BreakpointState> = this.breakpointObserver.observe(Breakpoints.HandsetPortrait);

    constructor(private breakpointObserver: BreakpointObserver) {
                    // this.checkLogin();
                    // this.authService.userLoggedIn.subscribe(x => {
                    //     this.checkLogin();
                    // });
                 }

    
}