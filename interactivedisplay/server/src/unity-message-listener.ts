import { UnityMessage } from './unity-message';

export interface UnityMessageListener {
    handler: (ev: UnityMessage) => void;
}
