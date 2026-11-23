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
package org.verapdf.cos.xref;

import org.verapdf.cos.COSKey;

import java.util.ArrayList;
import java.util.List;

/**
 * Tracks object keys known to the writer and allocates the next unused object
 * number after the greatest recorded key.
 *
 * <h2>.NET porting notes</h2>
 * In C#, represent {@code COSXRefTable} with immutable records containing numeric
 * object/generation identifiers and source offsets. Keep the xref table sparse (for example,
 * {@code IReadOnlyDictionary<COSKey, COSXRefEntry>}) and provide deterministic range/section
 * validation before resolving objects.
 */
public class COSXRefTable {

	private List<COSKey> all;
	private int maxKeyNumber;

	public COSXRefTable() {
		this.all = new ArrayList<>();
		maxKeyNumber = 0;
	}

	/**
	 * Replaces the tracked keys and recalculates the greatest object number.
	 *
	 * @param keys keys present in the cross-reference table
	 */
	public void set(final List<COSKey> keys) {
		this.all = keys;
		maxKeyNumber = keys.stream().map(COSKey::getNumber).max(Integer::compare).orElse(0);
	}

	private int getGreatestKeyNumberFromXref() {
		return maxKeyNumber;
	}

	/**
	 * Creates a key using the next object number and generation zero.
	 *
	 * @return next object key
	 */
	public COSKey next() {
		return new COSKey(getGreatestKeyNumberFromXref() + 1);
	}

	/**
	 * Adds a key and updates the greatest tracked object number when needed.
	 *
	 * @param key object key to record
	 */
	public void newKey(final COSKey key) {
		this.all.add(key);
		if (key.getNumber() > maxKeyNumber) {
			maxKeyNumber = key.getNumber();
		}
	}

	/**
	 * Adds several keys and updates the greatest tracked object number.
	 *
	 * @param key object keys to record
	 */
	public void newKey(final List<COSKey> key) {
		this.all.addAll(key);
		int newKeysMaxKeyNumber = key.stream().map(COSKey::getNumber).max(Integer::compare).orElse(0);
		if (newKeysMaxKeyNumber > maxKeyNumber) {
			maxKeyNumber = newKeysMaxKeyNumber;
		}
	}

	/**
	 * Returns the mutable list of keys tracked by this table.
	 *
	 * @return internal key list
	 */
	public List<COSKey> getAllKeys() {
		return this.all;
	}
}