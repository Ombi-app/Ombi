import { Component, OnInit } from "@angular/core";
import { map, startWith } from "rxjs/operators";

import { FormControl } from "@angular/forms";
import { IUserDropdown } from "../../../../interfaces";
import { IdentityService } from "../../../../services";
import { MatDialogRef } from "@angular/material/dialog";
import { Observable } from "rxjs";

@Component({
    selector: "request-behalf",
    templateUrl: "./request-behalf.component.html",
})
export class RequestBehalfComponent implements OnInit {
    constructor(
        public dialogRef: MatDialogRef<RequestBehalfComponent>,
        public identity: IdentityService) { }

    public myControl = new FormControl();
    public options: IUserDropdown[];
    public filteredOptions: Observable<IUserDropdown[]>;
    public userId: string;

    public async ngOnInit() {
        this.options = await this.identity.getUsersDropdown().toPromise();
        this.filteredOptions = this.myControl.valueChanges
            .pipe(
                startWith(''),
                map(value => this._filter(value))
            );
    }

    public request() {
        this.dialogRef.close(this.myControl.value);
    }

    public onNoClick(): void {
        this.dialogRef.close();
    }

    public displayFn(user: IUserDropdown): string {
        const username = user?.username ? user.username : '';
        const email = user?.email ? `(${user.email})` : '';
        return `${username} ${email}`;
      }

    private _filter(value: string|IUserDropdown): IUserDropdown[] {
        const filterValue = typeof value === 'string' ?  value.toLowerCase() : value.username.toLowerCase();

        return this.options.filter(option => option.username.toLowerCase().includes(filterValue));
    }
}
