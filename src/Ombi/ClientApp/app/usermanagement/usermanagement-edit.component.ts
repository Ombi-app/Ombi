import { Component, OnInit } from '@angular/core';

import { IUser, ICheckbox } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  
    templateUrl: './usermanagement-edit.component.html'
})
export class UserManagementEditComponent implements OnInit {
    constructor(private identityService: IdentityService, private route: ActivatedRoute) {
        this.route.params
            .subscribe(params => {
                this.userId = +params['id']; // (+) converts string 'id' to a number
            });
    }

    ngOnInit(): void {

        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        this.identityService.getUserById(this.userId).subscribe(x => this.user = x);
    }

    user: IUser;
    userId: number;
   

    availableClaims : ICheckbox[];

}