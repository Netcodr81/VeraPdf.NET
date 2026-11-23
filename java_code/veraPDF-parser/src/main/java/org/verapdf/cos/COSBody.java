/*
 * This file is part of veraPDF Parser, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Parser is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Parser as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Parser as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.cos;

import java.util.*;

/**
 * Stores the indirect objects that form a PDF document body, indexed by object
 * number and generation.
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code COSBody} is the low-level document aggregate and object lifetime boundary. It
 * should own indexed COS objects and disposable input state, offer an immutable header/trailer
 * view, and implement {@code IDisposable}; higher-level PD wrappers should depend on it rather
 * than duplicate parsing state.
 */
public class COSBody {

	private final Map<COSKey, COSObject> table;

	public COSBody() {
		this.table = new HashMap<>();
	}

	/**
	 * Returns a snapshot of the objects currently stored in the body.
	 *
	 * @return a new list containing the stored object wrappers
	 */
	public List<COSObject> getAll() {
		return new ArrayList<>(table.values());
	}

	/**
	 * Looks up an indirect object by key.
	 *
	 * @param key object number and generation
	 * @return the stored object, or a new empty wrapper when the key is absent
	 */
	public COSObject get(final COSKey key) {
		COSObject value = this.table.get(key);
		return value != null ? value : new COSObject();
	}

	/**
	 * Adds or replaces the body entry for {@code key}.
	 *
	 * @param key object number and generation
	 * @param object object wrapper to store
	 */
	public void set(final COSKey key, final COSObject object) {
		table.put(key, object);
	}

	/**
	 * Finds the key associated with an object. For direct values, lookup uses
	 * wrapper identity rather than COS value equality.
	 *
	 * @param obj object wrapper to locate
	 * @return its key, or {@code null} if a direct wrapper is not stored here
	 */
	public COSKey getKeyForObject(COSObject obj) {
		if (obj.isIndirect()) {
			return obj.getObjectKey();
		} else {
			for (Map.Entry<COSKey, COSObject> entry : this.table.entrySet()) {
				if (entry.getValue() == obj) {
                    return entry.getKey();
                }
			}
			return null;
		}
	}
}
