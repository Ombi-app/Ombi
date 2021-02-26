import { IssuesV2Service } from "../../services/issuesv2.service";
import { IdentityService, SearchService } from "../../services";
import { DetailsGroupComponent } from "./details-group/details-group.component";
import { IssuesDetailsComponent } from "./details/details.component";
import { IssueChatComponent } from "./issue-chat/issue-chat.component";
import { ChatBoxComponent } from "../../shared/chat-box/chat-box.component";



export const components: any[] = [
    DetailsGroupComponent,
    IssuesDetailsComponent,
    IssueChatComponent,
    ChatBoxComponent,
];

export const providers: any[] = [
    IssuesV2Service,
    IdentityService,
    SearchService,
];