import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { ICreateWizardUser } from "../../interfaces";
import { IdentityService, NotificationService } from "../../services";

@Component({
    templateUrl: "./welcome.component.html",
})
export class WelcomeComponent implements OnInit {
    
    public localUser: ICreateWizardUser;
  
    constructor(private router: Router,
        private identityService: IdentityService, private notificationService: NotificationService) { }

    public ngOnInit(): void {  
        this.localUser = {
            password:"",
            username:"",
            usePlexAdminAccount:false
        }
    }

    public createUser() {
        this.identityService.createWizardUser(this.localUser).subscribe(x => {
            if (x.result) {
                this.router.navigate(["login"]);
            } else {
                if (x.errors.length > 0) {
                    this.notificationService.error(x.errors[0]);
                }
            }
        });
    }
}
