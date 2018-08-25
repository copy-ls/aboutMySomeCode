
//Lyaer界面相关管理

let resInfo = function (path, layer_name, parent_node, callback) {
    return {
        path: path,
        layer_name: layer_name,
        parent_node: parent_node,
        callback: callback,
    }
}
//加锁
let Locks = cc.Class({
    statics: {

        locks: {

        },
        //获取单个加锁
        get(path) {
            return this.locks[path];
        },
        //设置加锁
        set(path, is_locked) {
            this.locks[path] = is_locked;
        },

        getAllLocks() {
            return this.locks;
        },

        lock(path) {
            this.set(path, true);
        },

        unLock(path) {
            this.set(path, false);
        },
    }
})

var layerMgr = cc.Class({

    ctor() {
        cc.director.on(cc.Director.EVENT_BEFORE_SCENE_LOADING, this.beforeSceneLoading.bind(this));
        cc.director.on(cc.Director.EVENT_AFTER_SCENE_LAUNCH, this.afterSceneLaunch.bind(this));
    },

    properties: {
        layers: {
            default: new Object(),
            type: Object,
            serializable: true,
        },
        layer_prefabs: {
            default: new Object(),
            type: Object,
            serializable: true,
        },
        loadingObjQueue: {
            default: new Object(),
            type: Object,
            serializable: true,
        },
        can_load_layer: false,
    },

    statics: {
        getInstance() {
            if (this.instance == null) {
                this.instance = new layerMgr();
                let runningScene = cc.director.getScene()
                if (runningScene != null) {
                    this.instance.can_load_layer = true;
                }
            }
            return this.instance
        },
    },

    beforeSceneLoading() {
        this.can_load_layer = false;
    },

    afterSceneLaunch() {
        for (const key in this.layer_prefabs) {
            if (this.layer_prefabs.hasOwnProperty(key)) {
                this.clearLayerPrefab(key, true);
            }
        }
        for (const key in this.layers) {
            if (this.layers.hasOwnProperty(key)) {
                this.clearLayer(key);
            }
        }
        this.clearLoadingObjQueue()
        this.can_load_layer = true;
    },

    getLayer(layer_name) {
        if (typeof layer_name === "string")
            return this.layers[layer_name];
        return null;
    },

    setLayer(layer_name, layer) {
        if (typeof layer_name === "string") {
            if (layer instanceof cc.Node) {
                this.layers[layer_name] = layer;
            }
            else if (layer == null) {
                delete this.layers[layer_name];
            }
        }
    },

    clearLayer(layer_name) {
        let layer = this.layers[layer_name];
        delete this.layers[layer_name];
        if (layer != null && layer instanceof cc.Node && cc.isValid(layer)) {
            console.log(`clear layer :${layer_name}`);
            layer.destroy();
        }
    },

    getLayerPrefab(layer_prefab_name) {
        if (typeof layer_prefab_name === "string")
            return this.layer_prefabs[layer_prefab_name];
        return null;
    },

    setLayerPrefab(layer_prefab_name, layer_prefab) {
        if (typeof layer_prefab_name === "string") {
            if (layer_prefab instanceof cc.Prefab) {
                this.layer_prefabs[layer_prefab_name] = layer_prefab;
            }
        }
    },

    clearLayerPrefab(layer_prefab_name, is_release_deps) {
        let layer_prefab = this.layer_prefabs[layer_prefab_name];
        delete this.layer_prefabs[layer_prefab_name];
        if (layer_prefab != null && layer_prefab instanceof cc.Prefab) {
            console.log(`clear layer prefab:${layer_prefab_name}`);
            if (is_release_deps === true) {
                let deps = cc.loader.getDependsRecursively(layer_prefab);
                console.log(`clear ${layer_prefab_name} deps`);
                cc.loader.release(deps)
            }
            cc.loader.releaseAsset(layer_prefab);
        }
    },

    pushLoadingObjQueue(path, layer_name, parent_node, callback) {
        if (this.loadingObjQueue[path] == null) {
            this.loadingObjQueue[path] = new Array();
        }
        this.loadingObjQueue[path].push(resInfo(path, layer_name, parent_node, callback));
    },

    shiftLoadingObjQueue(path) {
        if (this.loadingObjQueue[path] == null) {
            this.loadingObjQueue[path] = new Array();
        }
        return this.loadingObjQueue[path].shift();
    },

    clearLoadingObjQueue() {
        let all_locks = Locks.getAllLocks();
        for (const key in all_locks) {
            if (all_locks.hasOwnProperty(key)) {
                Locks.set(key, true);
                delete this.loadingObjQueue[key]
                delete all_locks[key];
            }
        }
    },

    loadLayer(path, layer_name, parent_node, callback) {
        if (this.can_load_layer == false) {
            console.log("场景切换中，不能加载" + layer_name);
            return
        }
        this.pushLoadingObjQueue(path, layer_name, parent_node, callback);
        if (Locks.get(path)) {
            return;
        }
        Locks.lock(path)
        this._loadLayer(path);
    },

    _loadLayer(path) {
        let res_info = this.shiftLoadingObjQueue(path);
        if (res_info == null) {
            return
        }
        let layer_name = res_info.layer_name;
        let parent_node = res_info.parent_node;
        let callback = res_info.callback;
        let layer = this.getLayer(layer_name)
        if (layer == null) {
            let layer_prefab_name = `${layer_name}Prefab`
            let layer_prefab = this.getLayerPrefab(layer_prefab_name);
            if (layer_prefab != null && layer_prefab instanceof cc.Prefab) {
                let runningScene = cc.director.getScene()
                if (runningScene == null) {
                    console.log("场景已经销毁");
                    return;
                }

                layer = cc.instantiate(layer_prefab);
                this.setLayer(layer_name, layer);

                if (parent_node != null && parent_node instanceof cc.Node) {
                    layer.parent = parent_node;
                } else {
                    layer.parent = runningScene.children[0];
                }
                layer.active = true;

                if (callback != null && typeof callback === "function") {
                    callback(layer);
                }

                Locks.unLock(path);
                this._loadLayer(path);
            }
            else {
                if (layer_prefab != null) {
                    this.clearLayerPrefab(layer_prefab_name);
                }
                cc.loader.loadRes(path, cc.Prefab, (err, prefab) => {
                    if (err) {
                        console.log(err);
                        Locks.unLock(path);
                        this._loadLayer(path)
                        return;
                    }
                    let runningScene = cc.director.getScene()
                    if (runningScene == null) {
                        console.log("场景已经销毁");
                        return;
                    }
                    this.setLayerPrefab(layer_prefab_name, prefab)
                    layer = cc.instantiate(prefab);

                    this.setLayer(layer_name, layer);
                    if (parent_node != null && parent_node instanceof cc.Node) {
                        layer.parent = parent_node;
                    } else {
                        layer.parent = runningScene.children[0];
                    }
                    layer.active = true;
                    if (callback != null && typeof callback === "function") {
                        callback(layer);
                    }
                    Locks.unLock(path);
                    this._loadLayer(path);
                })
            }
        }
        else if (layer instanceof cc.Node) {
            layer.setSiblingIndex(-1);
            layer.active = true;
            if (callback != null && typeof callback === "function") {
                callback(layer);
            }
            Locks.unLock(path);
            this._loadLayer(path);
        }
    },
});

exports = layerMgr;
