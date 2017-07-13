import { Component, OnInit } from '@angular/core';

import { IUser } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';

@Component({
  
    templateUrl: './usermanagement.component.html'
})
export class UserManagementComponent implements OnInit {
    constructor(private identityService: IdentityService) { }

    ngOnInit(): void {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });

    }

    users: IUser[];
    
   
}